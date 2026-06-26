# Phase 0 — Research: API_KEY do AbacatePay por Loja

Consolidação das decisões técnicas. Todos os pontos de NEEDS CLARIFICATION da spec já foram resolvidos na sessão de clarificação (2026-06-26); aqui resolvemos as incógnitas **de implementação**.

---

## R1. Estratégia de migration EF Core

**Decisão**: **Não gerar nova migration.** Apenas adicionar a propriedade `AbacatePayApiKey` à entidade `Store` e seu mapeamento Fluent em `ProxyPayContext`.

**Rationale**:
- A coluna física `proxypay_stores.abacatepay_api_key varchar(500) NULL` **já existe**, criada pela migration `20260324225618_AddStoreTable` (linha 42).
- O `ProxyPayContextModelSnapshot` **já contém** `AbacatePayApiKey` (linhas 254-257) — ou seja, o snapshot já reconhece a propriedade.
- Como a entidade `Store.cs` e `OnModelCreating` **não** mapeiam a propriedade hoje, o modelo está atrás do snapshot. Ao adicionar a propriedade + mapeamento, o modelo passa a **coincidir** com o snapshot para esse campo ⇒ um `dotnet ef migrations add` não geraria `AddColumn` para `abacatepay_api_key`.
- ⚠️ **Drift pré-existente**: o snapshot do `Store` **não** contém `ClientId` nem `BillingStrategy`, embora a entidade/contexto os mapeiem. Logo, gerar qualquer migration agora produziria `AddColumn client_id`/`billing_strategy` — colunas que já existem no banco — quebrando o apply. Por isso, **evitar** `dotnet ef migrations add` nesta feature.

**Alternativas consideradas**:
- *Gerar migration e editar manualmente o `Up` para no-op* — rejeitada: a constituição (§IV / Portões de Qualidade) exige migrations geradas via `dotnet ef` e não editadas manualmente; além disso reintroduziria o ruído do drift de `client_id`/`billing_strategy`.
- *Reconstruir o snapshot* — rejeitada nesta feature (escopo): corrigir o drift do snapshot é dívida técnica separada; mexer nele agora ampliaria o risco.

**Ação de acompanhamento (fora de escopo)**: registrar como tech-debt o drift entre `ProxyPayContextModelSnapshot` e a entidade `Store` (`client_id`/`billing_strategy` ausentes no snapshot).

---

## R2. Propagação da credencial por loja até o cliente AbacatePay

**Decisão**: Estender `IAbacatePayAppService` para receber a `apiKey` **por chamada**; os domain services (`InvoiceService`, `BillingService`) resolvem a credencial da loja antes de invocar o provedor.

Assinaturas novas:
```csharp
Task<AbacatePayResponse<BillingInfo>>      CreateBillingAsync(BillingCreateRequest request, string apiKey);
Task<AbacatePayResponse<PixQrCodeInfo>>    CreatePixQrCodeAsync(PixQrCodeCreateRequest request, string apiKey);
Task<AbacatePayResponse<PixQrCodeStatusInfo>> CheckStatusAsync(string id, string apiKey);
Task<AbacatePayResponse<PixQrCodeInfo>>    SimulatePaymentAsync(string id, string apiKey);
```
`AbacatePayAppService.CreateClient(string apiKey)` passa a usar a chave recebida no header `Authorization: Bearer {apiKey}`.

**Rationale**:
- `InvoiceService.CreateInvoiceAsync/CreateQRCodeAsync` e `BillingService.CreateBillingAsync` já recebem `storeId` ⇒ basta resolver a loja e ler a chave.
- `CheckQRCodeStatusAsync`/`SimulatePaymentAsync` recebem `invoiceId` ⇒ resolvem `storeId` a partir da `Invoice` e então a chave da loja.
- `ProcessWebhookAsync` **não** chama o AbacatePay (apenas atualiza status local) ⇒ não precisa de credencial.
- O `AbacatePayAppService` deixa de depender obrigatoriamente de `AbacatePaySetting.ApiKey` global; o `AbacatePaySetting.ApiUrl` continua sendo configuração de ambiente.

**Resolução da chave da loja**: injetar `IStoreRepository<StoreModel>` nos domain services de pagamento e usar `GetByIdAsync(storeId)`; ler `StoreModel.AbacatePayApiKey`.

**Alternativas consideradas**:
- *Resolver a chave dentro do `AbacatePayAppService` (Infra) via tenant/store context* — rejeitada: acoplaria a camada de integração ao repositório de Store e ao contexto HTTP; manter a resolução no domain (que já tem `storeId`) é mais limpo.
- *Usar `HttpClientFactory` com handler que injeta a chave* — rejeitada: o serviço já cria `HttpClient` manualmente (com bypass de cert); a chave varia por requisição, não por cliente nomeado.

---

## R3. Comportamento quando a loja não tem credencial

**Decisão**: Recusar a operação de pagamento com erro claro **antes** de chamar o provedor; **sem** fallback para `AbacatePaySetting.ApiKey`.

**Rationale**: Clarificação 2026-06-26 (FR-014). Evita que pagamentos de uma loja caiam na conta AbacatePay do ambiente. O domain service valida `string.IsNullOrWhiteSpace(store.AbacatePayApiKey)` e lança exceção (`"Store has no AbacatePay credential configured"`), que o controller converte em `BadRequest` (Padrão 1 de erro).

**Impacto operacional**: lojas em produção (emagine, monexup, fortuno) precisam cadastrar a credencial por loja antes de continuar transacionando — comunicar janela de migração (ver quickstart).

---

## R4. Ocultar a credencial no GraphQL e expor indicador booleano

**Decisão**: Criar `StoreType : ObjectType<Store>` que (a) `Ignore()` o campo `AbacatePayApiKey` e (b) adiciona o campo computado `hasAbacatePayApiKey: Boolean!`. Registrar com `.AddType<StoreType>()` em `GraphQLServiceExtensions`.

Resolver do indicador (evitando armadilha de projeção):
```csharp
descriptor.Field("hasAbacatePayApiKey")
    .Type<NonNullType<BooleanType>>()
    .Resolve(ctx =>
    {
        var parent = ctx.Parent<Store>();
        var context = ctx.Service<ProxyPayContext>();
        return context.Stores
            .Where(s => s.StoreId == parent.StoreId)
            .Select(s => s.AbacatePayApiKey != null && s.AbacatePayApiKey != "")
            .FirstOrDefault();
    });
descriptor.Field(s => s.AbacatePayApiKey).Ignore();
```

**Rationale**:
- A query `myStore` retorna `IQueryable<Store>` com `[UseProjection]`. Se a propriedade existir no tipo, ela seria projetada/exposta. `Ignore()` remove o campo do schema (não é consultável).
- Como o campo é ignorado, ele **não é projetado**; por isso o resolver do booleano consulta o contexto por `StoreId` em vez de depender de `ctx.Parent<Store>().AbacatePayApiKey` (que viria nulo após projeção).

**Alternativas consideradas**:
- *Atributo `[GraphQLIgnore]` direto na propriedade da entidade* — rejeitada: poluiria a entidade de Infra com anotação de apresentação GraphQL; o projeto já isola isso em `Types/`.
- *Não expor indicador nenhum* — rejeitada pela clarificação (FR-013): o painel admin precisa saber se a loja está configurada.

---

## R5. Proteção do update geral contra sobrescrita da credencial

**Decisão**: Em `StoreProfile`, adicionar `.ForMember(d => d.AbacatePayApiKey, opt => opt.Ignore())` nos mapas `StoreInsertInfo → StoreModel` e `StoreUpdateInfo → StoreModel`.

**Rationale**: `StoreService.UpdateAsync` faz `_mapper.Map(store, existing)`. Sem o `Ignore()`, como `StoreUpdateInfo` não possui `apiKey`, o AutoMapper sobrescreveria `existing.AbacatePayApiKey` com `null`, apagando a credencial a cada update geral — violando FR-007/FR-008. O `Ignore()` garante que somente o endpoint dedicado altere a credencial.

---

## R6. Não-exposição via REST e logs

**Decisão**:
- `StoreInfo` (DTO de leitura) **não** ganha o campo da credencial — permanece como está (sem `AbacatePayApiKey`). Nenhum endpoint REST retorna a credencial.
- A `apiKey` **não** é incluída em nenhum `ILogger` (nem no endpoint de alteração nem no `AbacatePayAppService`).

**Rationale**: FR-005/FR-006 e §V da constituição (secrets nunca em respostas/logs). O `AbacatePayAppService` hoje loga `Request`/`Response` do provedor, mas a `apiKey` vai apenas no header — confirmar que nenhum log novo a inclua.

---

## R7. Validação de entrada (endpoint dedicado)

**Decisão**: Validar `apiKey` não vazia. Implementar via `StoreModel.SetAbacatePayApiKey(string)` (trim + guarda contra vazio) e, para consistência com o projeto, um `FluentValidation` `StoreApiKeyUpdateInfoValidator` (skill `dotnet-fluent-validation`). Sem validação on-line contra o AbacatePay (FR-016).

**Rationale**: FR-008 (rejeitar vazio sem apagar valor existente). A normalização (trim) evita credencial inválida por espaços (Edge Case da spec).

---

## Resumo das decisões

| # | Decisão | Requisito |
|---|---------|-----------|
| R1 | Sem nova migration; mapear propriedade existente | FR-001 |
| R2 | `apiKey` por chamada em `IAbacatePayAppService`; resolução no domain | FR-010 |
| R3 | Sem fallback; recusa clara quando ausente | FR-014 |
| R4 | `StoreType` oculta campo + `hasAbacatePayApiKey` | FR-005, FR-006, FR-013 |
| R5 | AutoMapper `Ignore()` protege contra wipe no update geral | FR-007, FR-008 |
| R6 | Sem exposição em REST/logs | FR-005, FR-006 |
| R7 | Validação de não-vazio + trim, sem validação on-line | FR-008, FR-016 |
