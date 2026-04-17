# Phase 0 Research: Multi-Tenant "fortuno"

Este documento resolve as decisões técnicas não-triviais antes de projetar
dados e contratos. Cada entrada segue o formato **Decision / Rationale /
Alternatives considered**.

---

## R-001. Reutilizar a infraestrutura multi-tenant já presente no ProxyPay

**Decision**: Preservar e completar os componentes existentes
(`TenantMiddleware`, `TenantContext`, `TenantResolver`,
`TenantDbContextFactory`, `NAuthTenantSecretProvider`,
`NAuthTenantProvider`) em vez de reescrever do zero. Skill
`dotnet-multi-tenant` e projeto `c:\repos\Lofn\Lofn` servirão de referência
para fechar gaps, não para criar paralelos.

**Rationale**: A infraestrutura atual do ProxyPay é estrutural e
semanticamente idêntica à do Lofn (mesmos nomes de classes, mesma assinatura
de `ITenantResolver`, mesma lookup em `Tenants:{id}:{campo}`). Reescrever
introduziria risco sem benefício.

**Alternatives considered**:
- Reescrever a camada de tenant a partir da skill como novo set de arquivos
  — **rejeitado** por retrabalho e risco de duplicação.
- Substituir por uma abordagem shared-DB com coluna `tenant_id` — **rejeitado**
  pela assumption explícita da spec: isolamento físico por banco.

---

## R-002. Cabeçalho HTTP canônico: `X-Tenant-Id`

**Decision**: Usar o cabeçalho `X-Tenant-Id` como fonte única de verdade
para o tenant em toda requisição (autenticada ou anônima). Casing do nome
do cabeçalho é case-insensitive no HTTP; valor do cabeçalho é tratado como
string opaca (sem validação de formato, conforme clarificação da spec).

**Rationale**: É o nome já usado em `TenantMiddleware.cs` do ProxyPay e em
Lofn. Manter este nome evita breaking change para clientes internos já
integrados e alinha com o `TenantHeaderHandler` do Lofn.

**Alternatives considered**:
- `Tenant`, `X-Tenant`, `X-ProxyPay-Tenant` — **rejeitado** por divergir do
  padrão Lofn e do código atual sem ganho.
- Claim `tenant` dentro do JWT — **rejeitado** porque não funciona para
  tráfego anônimo (GraphQL público, webhooks).
- Subdomínio — **rejeitado** em clarificação Q2 da spec.

---

## R-003. Enforcement do cabeçalho: rejeição explícita, sem fallback

**Decision**:
1. `TenantMiddleware` passa a rejeitar imediatamente (HTTP 400) toda
   requisição sem header `X-Tenant-Id` ou com tenant desconhecido, **antes
   de** `UseAuthentication`.
2. `TenantResolver.TenantId` passa a **não cair mais** em `DefaultTenantId`
   quando o context está vazio — retorna erro ou lança `InvalidOperationException`
   se chamado fora de um escopo de requisição.
3. `Tenant:DefaultTenantId` no appsettings fica como conveniência apenas
   para ferramentas offline (ex.: runner de migrações que não tem HTTP
   context), não para tráfego da API.
4. Rotas de health check (`/`) e Swagger (`/swagger/*`) são **exemption
   list** — atendidas sem enforcement de tenant.

**Rationale**: FR-004 exige rejeição de toda requisição sem contexto de
tenant resolvível. O fallback silencioso atual (`??= DefaultTenantId` no
`TenantResolver`) violaria essa regra — uma requisição sem header cairia
implicitamente em `emagine`, que é exatamente o anti-padrão da spec. A
exemption de health/swagger mantém observabilidade e doc sem quebrar
contratos operacionais.

**Alternatives considered**:
- Enforcement só em endpoints autenticados (manter fallback em anônimos) —
  **rejeitado** pela clarificação Q2 (header obrigatório em toda requisição).
- Enforcement via `[TenantRequired]` attribute por controller — **rejeitado**
  porque fácil esquecer; middleware global é mais seguro.

---

## R-004. Endpoints públicos (Payment, Webhook) — estratégia de tenant

**Decision**:

- **PaymentController**: passa a exigir `X-Tenant-Id` como qualquer outro
  endpoint. O `ClientId` no payload continua sendo usado para localizar a
  `Store`, mas a resolução agora ocorre no `ProxyPayContext` do tenant da
  requisição — impossível vazar entre tenants porque o `DbContext` aponta
  para bancos físicos distintos.
- **WebhookController** (AbacatePay): AbacatePay é externo e não sabe sobre
  tenants. Decisão: **URL de webhook passa a ser por tenant**, no formato
  `/webhook/abacatepay/{tenantId}`. Cada tenant registra sua URL
  independentemente no painel da AbacatePay com seu próprio
  `WebhookSecret`. O middleware de tenant **não** lê o header neste
  endpoint — lê o path segment `{tenantId}` e o injeta no `HttpContext.Items`
  antes de seguir o pipeline.

**Rationale**: O AbacatePay não aceita cabeçalhos customizados em webhooks
(comportamento padrão de provedores de pagamento). Path-based tenant routing
é a convenção consolidada para webhooks multi-tenant em SaaS. Como cada
tenant já tem seu próprio `WebhookSecret` na config (chave
`AbacatePay:WebhookSecret`), basta mover esse segredo para
`Tenants:{tenantId}:AbacatePayWebhookSecret` e validar contra o tenant da
rota.

**Alternatives considered**:
- Resolver tenant a partir do ID do invoice no payload — **rejeitado** porque
  exigiria índice global cross-tenant (contraria R-001 e a assumption de
  bancos fisicamente isolados).
- Um único segredo compartilhado com inspeção de payload — **rejeitado** por
  violar FR-006 (segredos por tenant).

---

## R-005. Tenant catalog — fonte da verdade

**Decision**: Manter catálogo de tenants em `appsettings.{Environment}.json`
sob `Tenants:{tenantId}:{ConnectionString|JwtSecret|BucketName|AbacatePayWebhookSecret}`.
Introduzir `ITenantCatalog` / `TenantCatalog` como wrapper de leitura tipada
(retorna `IEnumerable<string>` de tenant IDs válidos) para suportar o
enforcement em middleware e o runner de migrações. Sem tabela master em
banco nesta entrega.

**Rationale**: Simetria com o Lofn; zero infra nova; rotação de segredo é
edição de appsettings + reload de config (já suportado pelo ASP.NET Core
via `IConfigurationRoot.Reload()` ou reload automático em `appsettings`
monitorados). Nenhum overhead de consulta a banco por requisição.

**Alternatives considered**:
- Tabela master `tenants` em um banco compartilhado — **rejeitado** por
  adicionar um banco "secreto" que contradiz a assumption de isolamento
  por-banco e exige políticas de HA específicas para esse banco especial.
- Secrets manager (AWS SSM, Azure KeyVault) — **deferido** como follow-up
  de hardening; o padrão atual (env vars injetando em appsettings) é
  suficiente para a entrega inicial e já praticado no Lofn em produção.

---

## R-006. Runner de migrações cross-tenant

**Decision**: Adicionar um comando CLI dentro da própria API
(`ProxyPay.API` ganha um argumento `--migrate-all-tenants`) que:
1. Lê `ITenantCatalog`
2. Para cada tenant, instancia `ProxyPayContext` via factory com a
   connection string do tenant
3. Executa `context.Database.Migrate()`
4. Registra stdout estruturado: tenant, migrations aplicadas, duração,
   resultado
5. Falha rápida no primeiro erro com código de saída não-zero

A execução do comando é manual (parte do procedimento de deploy), não
automática no startup da API — evita efeitos colaterais em cold start.

**Rationale**: Mantém toda a lógica dentro do próprio binário
`ProxyPay.API`, usa a mesma factory de produção e a mesma
`ProxyPayContext` — zero divergência entre schema produzido localmente e em
produção. Produz log auditável exigido por FR-015. Simples de acionar no
pipeline de deploy (`dotnet run --project ProxyPay.API --
--migrate-all-tenants`).

**Alternatives considered**:
- Projeto console separado `ProxyPay.Migrator` — **rejeitado** por
  duplicar bootstrap/DI sem ganho.
- `context.Database.Migrate()` no startup da API — **rejeitado** por
  timing perigoso (uma instância faz migration enquanto outras já tentam
  servir tráfego) e por dificultar rollback controlado.
- `dotnet ef database update --connection <cs>` por tenant via script
  shell — **rejeitado** porque exige o CLI `dotnet-ef` instalado no ambiente
  de deploy e perde a auditoria estruturada.

---

## R-007. Propagação outbound do tenant (ACL)

**Decision**: Registrar um `TenantHeaderHandler : DelegatingHandler` em DI
e anexá-lo aos `HttpClient` gerenciados usados pelos ACL clients
(`UserClient`, `FileClient`, `ChatGPTClient`, `MailClient`, `StringClient`,
`DocumentClient`, `IAbacatePayAppService`). O handler lê o tenant corrente
via `ITenantContext` e adiciona `X-Tenant-Id: {tenantId}` ao request
outbound. Na ausência de `ITenantContext.TenantId` (ex.: chamadas do runner
de migrações), o handler não adiciona o header e loga warning.

**Rationale**: FR-008 exige propagação automática. Sem isso, os serviços
externos (NAuth, zTools) não conseguem manter o isolamento por tenant
quando ProxyPay atua como cliente deles. Padrão é idêntico ao Lofn.

**Alternatives considered**:
- Setar header manualmente em cada client — **rejeitado** por ser passível
  de esquecimento (mesma falha categoricamente equivalente à que US2
  procura eliminar).
- Injetar `ITenantContext` em cada client — **rejeitado** por quebrar o
  abstraction boundary dos pacotes NAuth/zTools (Lib/ externas).

---

## R-008. Migração da base atual → banco dedicado de "emagine"

**Decision**: Procedimento one-shot de migração será realizado **fora** do
runtime da aplicação, como etapa do plano de deploy:
1. Parar o tráfego em produção (janela de manutenção curta).
2. Criar novo banco PostgreSQL dedicado: `proxypay_emagine`.
3. `pg_dump` da base atual `proxypay_db` → restore em `proxypay_emagine`.
4. Criar novo banco dedicado: `proxypay_fortuno` (vazio).
5. Atualizar `appsettings.Production.json` com as connection strings novas,
   adicionando o tenant `fortuno` e removendo `monexup`.
6. Aplicar migrations via `--migrate-all-tenants` para garantir que ambos
   os bancos estejam no schema mais recente.
7. Reabrir tráfego. Frontend passa a enviar `X-Tenant-Id: emagine` (ou a
   política de deployment do cliente web — tratada fora deste plano).

**Rationale**: Clone físico do banco existente é a abordagem de menor
risco e mais simples de verificar (diff de rowcounts por tabela). O
procedimento é reversível (rollback = repointar connection string em
appsettings). A janela de manutenção é operacionalmente aceitável porque o
ProxyPay é backoffice/B2B, não 24/7 de consumo massivo.

**Alternatives considered**:
- Replicação online via `pg_logical` / `CREATE SUBSCRIPTION` — **rejeitado**
  por complexidade injustificada em uma migração one-shot pontual.
- Manter o banco atual como banco de `emagine` (sem clone) — **rejeitado**
  porque o banco atual se chama `proxypay_db`, nome genérico que não
  reflete a nova semântica; cleanup de naming é mais barato agora do que
  depois.

---

## R-009. Tratamento indistinguível de tokens de outro tenant

**Decision**: Com JWT validado por segredo do tenant (via NAuth +
`NAuthTenantSecretProvider`), um token assinado pelo segredo de tenant X
apresentado com `X-Tenant-Id: Y` falhará na validação de assinatura —
produzindo automaticamente o mesmo erro `401 Unauthorized` que um token
inválido genérico gera. Confirmar em teste que:
- Token expirado → `401`
- Token de outro tenant → `401`
- Token assinado com segredo desconhecido → `401`
As três respostas DEVEM ter o mesmo body e headers. Nenhum custom error
message que leak tenant info.

**Rationale**: FR-012 exige indistinguibilidade para bloquear enumeração
por oracle. NAuth já comporta-se dessa forma; o risco é apenas o
`GlobalExceptionFilter` injetar mensagens customizadas — precisamos
auditar.

**Alternatives considered**:
- Erros específicos (`TENANT_MISMATCH` vs `TOKEN_EXPIRED`) — **rejeitado**
  explicitamente por ser o anti-padrão que FR-012 combate.

---

## R-010. Reconfigurar CORS? (coberto por exceção transitória da constituição)

**Decision**: Manter comportamento atual do `Startup.cs`
(`AllowAnyOrigin`). **Não** alterar CORS como parte desta feature.

**Rationale**: A constituição v1.1.0 (emendada em 2026-04-17) §V registra
**exceção transitória explícita, datada até 2026-07-31**, reconhecendo
`AllowAnyOrigin` em todos os ambientes do repositório ProxyPay como dívida
técnica aceita. Enquanto a exceção estiver vigente, o comportamento não
configura violação — apenas débito rastreável. Corrigir CORS durante esta
feature continua sendo scope creep (a feature não toca o pipeline de CORS
nem o aproxima/afasta da conformidade), e a remediação fica endereçada
para uma feature dedicada antes da expiração da exceção.

**Alternatives considered**:
- Corrigir CORS junto — **rejeitado** por scope creep; a dívida está
  formalizada e datada na constituição (não é mais um follow-up ambíguo).

**Follow-up obrigatório (fora desta feature)**:
- Antes de **2026-07-31**: abrir feature dedicada
  ("fix CORS conditional by environment") que (a) restrinja origens em
  `Docker` e `Production` no `ProxyPay.API/Startup.cs`, (b) remova a
  exceção transitória de §V via emenda PATCH (`v1.1.1`), (c) atualize
  esta entrada R-010 para marcar como "RESOLVIDO".

---

## Resumo de NEEDS CLARIFICATION

Nenhum. Todos os pontos levantados no Technical Context foram resolvidos
nesta fase 0 — as quatro clarificações da spec já haviam fechado as
ambiguidades de escopo, e os dez pontos acima fecham as decisões
implementacionais.
