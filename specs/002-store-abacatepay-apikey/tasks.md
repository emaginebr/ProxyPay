---
description: "Task list for feature: API_KEY do AbacatePay por Loja"
---

# Tasks: API_KEY do AbacatePay por Loja

**Input**: Design documents from `/specs/002-store-abacatepay-apikey/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: O spec **não** solicitou TDD. As tarefas de teste aparecem apenas na fase de Polish e estão marcadas como **OPCIONAIS** (recomendadas pela natureza de segurança/secret da feature).

**Organization**: Tarefas agrupadas por user story. As stories US1 e US2 são P1; US3 é P2; US4 é P3.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências pendentes)
- **[Story]**: US1 / US2 / US3 / US4

## Convenção de skills (constituição §I)

Toda criação/alteração de entidade, model, DTO, service, mapper e DI **DEVE** usar a skill `dotnet-architecture`; o validator novo usa `dotnet-fluent-validation`; testes usam `dotnet-test`. Migrations: **NÃO** gerar (ver R1 do research.md — coluna e snapshot já existem).

---

## Phase 1: Setup

**Purpose**: Linha de base antes das alterações.

- [X] T001 Garantir build limpo da baseline com `dotnet build backend/ProxyPay.sln` (confirmar que o branch compila antes de iniciar)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Persistência do atributo `AbacatePayApiKey` — pré-requisito compartilhado por US1, US2 e US3.

**⚠️ CRITICAL**: Nenhuma user story pode ser concluída antes desta fase. **NÃO** rodar `dotnet ef migrations add` (a coluna `abacatepay_api_key` e a entrada no `ModelSnapshot` já existem; ver research.md R1).

- [X] T002 [P] Adicionar a propriedade `string AbacatePayApiKey` à entidade em `backend/ProxyPay.Infra/Context/Store.cs`
- [X] T003 [P] Mapear a coluna `abacatepay_api_key` (`HasMaxLength(500)`, `HasColumnName`) no bloco `modelBuilder.Entity<Store>` de `backend/ProxyPay.Infra/Context/ProxyPayContext.cs`
- [X] T004 [P] Adicionar `AbacatePayApiKey` e o método `SetAbacatePayApiKey(string)` (trim + guarda contra vazio, `MarkUpdated()`) em `backend/ProxyPay.Domain/Models/StoreModel.cs`
- [X] T005 [P] Em `backend/ProxyPay.Infra/Mappers/StoreProfile.cs`, adicionar `.ForMember(d => d.AbacatePayApiKey, opt => opt.Ignore())` aos mapas `StoreInsertInfo → StoreModel` e `StoreUpdateInfo → StoreModel` (impede o update geral de apagar a credencial)

**Checkpoint**: A propriedade existe em todas as camadas de persistência; o update geral não a sobrescreve. O solution compila (`dotnet build backend/ProxyPay.sln`).

---

## Phase 3: User Story 1 - Definir/alterar a API_KEY (Priority: P1) 🎯 MVP

**Goal**: Endpoint dedicado que define/substitui a credencial do AbacatePay de uma loja, autenticado e restrito ao dono.

**Independent Test**: `PUT /store/{storeId}/abacatepay-apikey` com `apiKey` válida em loja própria retorna `204`; tentativas sem auth/dono/valor retornam `401`/`403`/`400`. (Ver `contracts/set-abacatepay-apikey.http.md`.)

### Implementation for User Story 1

- [X] T006 [P] [US1] Criar o DTO `StoreApiKeyUpdateInfo` (`apiKey` com `[JsonPropertyName("apiKey")]`) em `backend/ProxyPay.DTO/Store/StoreApiKeyUpdateInfo.cs`
- [X] T007 [P] [US1] Criar `StoreApiKeyUpdateInfoValidator` (regra: `ApiKey` `NotEmpty`) em `backend/ProxyPay.Domain/Validators/StoreApiKeyUpdateInfoValidator.cs` (auto-registrado via `AddValidatorsFromAssemblyContaining` em `ProxyPay.Application/Startup.cs`)
- [X] T008 [P] [US1] Adicionar `Task UpdateAbacatePayApiKeyAsync(long storeId, string apiKey, long userId)` em `backend/ProxyPay.Domain/Interfaces/IStoreService.cs`
- [X] T009 [US1] Implementar `UpdateAbacatePayApiKeyAsync` em `backend/ProxyPay.Domain/Services/StoreService.cs` (GetByIdAsync → "Store not found" se nulo; `ValidateOwnership(userId)`; `SetAbacatePayApiKey(apiKey)`; `UpdateAsync`) — depende de T004, T008
- [X] T010 [US1] Adicionar o endpoint `[HttpPut("{storeId}/abacatepay-apikey")] SetAbacatePayApiKey(long storeId, [FromBody] StoreApiKeyUpdateInfo info)` em `backend/ProxyPay.API/Controllers/StoreController.cs` (sessão→`Unauthorized`; `UnauthorizedAccessException`→`Forbid`; `Exception`→`BadRequest`; sucesso→`NoContent`; **não logar** `apiKey`) — depende de T006, T009

**Checkpoint**: US1 funcional e testável de forma independente (MVP).

---

## Phase 4: User Story 2 - Credencial nunca legível + indicador (Priority: P1)

**Goal**: Garantir que o valor da credencial não seja exposto por nenhuma leitura (REST/GraphQL) e expor apenas o booleano `hasAbacatePayApiKey`.

**Independent Test**: `query { myStore { hasAbacatePayApiKey } }` retorna `true`/`false`; `query { myStore { abacatepayApiKey } }` falha com campo desconhecido. (Ver `contracts/store-graphql.contract.md`.)

### Implementation for User Story 2

- [X] T011 [P] [US2] Criar `StoreType : ObjectType<Store>` em `backend/ProxyPay.GraphQL/Types/StoreType.cs` — `descriptor.Field(s => s.AbacatePayApiKey).Ignore()` e campo computado `hasAbacatePayApiKey` (`NonNullType<BooleanType>`) resolvido via `ProxyPayContext` por `StoreId` (evita armadilha de projeção)
- [X] T012 [US2] Registrar `.AddType<StoreType>()` em `AddProxyPayGraphQL` de `backend/ProxyPay.GraphQL/GraphQLServiceExtensions.cs` — depende de T011
- [X] T013 [P] [US2] Verificar que `backend/ProxyPay.DTO/Store/StoreInfo.cs` (e qualquer resposta REST de loja) **não** inclui a credencial — confirmar ausência; nenhuma alteração esperada

**Checkpoint**: Valor da credencial não consultável; indicador booleano disponível. US1 + US2 funcionam de forma independente.

---

## Phase 5: User Story 3 - Pagamentos usam a credencial da loja (Priority: P2)

**Goal**: Operações de pagamento (Invoice/Billing/QR Code/Check/Simulate) usam a credencial da loja envolvida; sem credencial ⇒ erro claro, sem fallback para a chave de ambiente.

**Independent Test**: Duas lojas com chaves distintas ⇒ cada pagamento usa a sua; loja sem chave ⇒ `400` com erro claro, sem chamada ao provedor com chave global. (Ver `quickstart.md` passo 5.)

### Implementation for User Story 3

- [X] T014 [US3] Adicionar o parâmetro `string apiKey` a todos os métodos de `backend/ProxyPay.Infra.Interfaces/AppServices/IAbacatePayAppService.cs` (`CreateBillingAsync`, `CreatePixQrCodeAsync`, `CheckStatusAsync`, `SimulatePaymentAsync`)
- [X] T015 [US3] Atualizar `backend/ProxyPay.Infra/AppServices/AbacatePayAppService.cs`: `CreateClient(string apiKey)` usa a chave recebida no header `Bearer`; propagar `apiKey` por `PostAsync`/`GetAsync` e métodos públicos; manter `_settings.ApiUrl`; **não** logar a `apiKey` — depende de T014
- [X] T016 [US3] Em `backend/ProxyPay.Domain/Services/BillingService.cs`: injetar `IStoreRepository<StoreModel>`; em `CreateBillingAsync(..., storeId, ...)` resolver `store = GetByIdAsync(storeId)`, lançar erro claro se `string.IsNullOrWhiteSpace(store.AbacatePayApiKey)` (sem fallback), e passar a chave para `_abacatePayAppService.CreateBillingAsync(req, store.AbacatePayApiKey)` — depende de T014, T015, T004
- [X] T017 [US3] Em `backend/ProxyPay.Domain/Services/InvoiceService.cs`: injetar `IStoreRepository<StoreModel>`; resolver a chave por `storeId` em `CreateInvoiceAsync`/`CreateQRCodeAsync` e por `invoice.StoreId` em `CheckQRCodeStatusAsync`/`SimulatePaymentAsync`; lançar erro claro se ausente (sem fallback); passar a chave em todas as chamadas ao `_abacatePayAppService` — depende de T014, T015, T004

**Checkpoint**: Pagamentos por loja autenticados com a chave correta; ausência de chave recusada com clareza. `ProcessWebhookAsync` permanece intacto (não chama o provedor).

---

## Phase 6: User Story 4 - Coleção Bruno (Priority: P3)

**Goal**: Disponibilizar a requisição do endpoint dedicado na coleção Bruno.

**Independent Test**: Abrir `bruno/Store/` e executar a request contra uma loja real ⇒ `204`.

### Implementation for User Story 4

- [X] T018 [P] [US4] Criar `bruno/Store/Set ApiKey.bru` (método `put` para `{{baseUrl}}/store/1/abacatepay-apikey`, header `X-Tenant-Id: emagine`, `auth: bearer` com `{{token}}`, body `{ "apiKey": "..." }`, `seq` seguinte ao da pasta Store)

**Checkpoint**: Todas as user stories independentemente funcionais.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [X] T019 [P] **(OPCIONAL)** Testes xUnit via skill `dotnet-test` no projeto `.Tests`: `StoreModel.SetAbacatePayApiKey` (trim e rejeição de vazio), `StoreService.UpdateAbacatePayApiKeyAsync` (ownership e loja inexistente), AutoMapper não apaga a chave no update geral, e pagamento lança erro quando a loja não tem chave (sem fallback)
- [X] T020 [P] Revisar logging para garantir que a `apiKey` **nunca** apareça em `ILogger` — `backend/ProxyPay.API/Controllers/StoreController.cs` e `backend/ProxyPay.Infra/AppServices/AbacatePayAppService.cs`
- [X] T021 Build final `dotnet build backend/ProxyPay.sln` e executar a verificação manual de `specs/002-store-abacatepay-apikey/quickstart.md` (passos 1–6)
- [X] T022 [P] Registrar como tech-debt o drift do `backend/ProxyPay.Infra/Migrations/ProxyPayContextModelSnapshot.cs` (faltam `client_id`/`billing_strategy` no snapshot do `Store`) — não corrigir nesta feature

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sem dependências
- **Foundational (Phase 2)**: depende do Setup — **BLOQUEIA** todas as user stories
- **US1 (Phase 3)** e **US2 (Phase 4)**: dependem da Foundational; independentes entre si
- **US3 (Phase 5)**: depende da Foundational (precisa de `StoreModel.AbacatePayApiKey`, T004); independente de US1/US2
- **US4 (Phase 6)**: documenta US1; idealmente após T010, mas o arquivo Bruno pode ser escrito a qualquer momento
- **Polish (Phase 7)**: após as stories desejadas

### User Story Dependencies

- **US1 (P1)**: após Foundational — independente
- **US2 (P1)**: após Foundational (a propriedade precisa existir para ser ocultada) — independente de US1
- **US3 (P2)**: após Foundational (T004) — independente de US1/US2
- **US4 (P3)**: descreve o endpoint de US1

### Within Each User Story

- US1: T006/T007/T008 [P] → T009 → T010
- US2: T011 → T012; T013 [P] em paralelo
- US3: T014 → T015 → (T016, T017)

### Parallel Opportunities

- Foundational: T002, T003, T004, T005 em paralelo (arquivos distintos)
- US1: T006, T007, T008 em paralelo
- US3 e (US1/US2) podem ser tocadas por pessoas diferentes após a Foundational
- US4 (T018) e Polish [P] independentes

---

## Parallel Example: Foundational

```text
# Após T001, lançar em paralelo:
T002: Propriedade em Store.cs
T003: Mapeamento Fluent em ProxyPayContext.cs
T004: AbacatePayApiKey + SetAbacatePayApiKey em StoreModel.cs
T005: Ignore() nos mapas DTO→Model em StoreProfile.cs
```

## Parallel Example: User Story 1

```text
T006: DTO StoreApiKeyUpdateInfo
T007: StoreApiKeyUpdateInfoValidator
T008: Assinatura em IStoreService
# depois: T009 (StoreService) → T010 (Controller)
```

---

## Implementation Strategy

### MVP First (US1)

1. Phase 1 (Setup) → Phase 2 (Foundational) → Phase 3 (US1)
2. **STOP & VALIDATE**: exercitar `PUT /store/{storeId}/abacatepay-apikey` (contrato REST). MVP = é possível cadastrar/alterar a credencial por loja.

### Incremental Delivery

1. Foundational pronto
2. + US1 → cadastro/alteração da credencial (MVP)
3. + US2 → garantia write-only + indicador no GraphQL
4. + US3 → pagamentos por loja (⚠️ exige cadastrar a chave de cada loja em produção antes — ver quickstart "Nota de migração")
5. + US4 → Bruno
6. Polish → testes/segurança/validação

---

## Notes

- [P] = arquivos diferentes, sem dependências pendentes
- **Nenhuma migration EF** nesta feature (research.md R1)
- A `apiKey` é secret: **nunca** em respostas nem logs (constituição §V)
- US3 remove o fallback global ⇒ comunicar janela de cadastro para emagine/monexup/fortuno
- Commitar após cada tarefa ou grupo lógico; validar cada story no checkpoint
