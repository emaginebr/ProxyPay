---
description: "Task list for Multi-Tenant com tenant inicial \"fortuno\""
---

# Tasks: Multi-Tenant com tenant inicial "fortuno"

**Input**: Design documents from `C:\repos\ProxyPay\ProxyPay\specs\001-multi-tenant-fortuno\`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Test tasks são **incluídas** porque os critérios de sucesso
(SC-001, SC-003, SC-006) exigem explicitamente "auditoria independente",
"testes sistemáticos de reutilização de token" e "teste controlado" de
rotação de segredo. Esta é uma feature de segurança crítica e o projeto
`ProxyPay.Tests` (xUnit) já existe. Tests aparecem marcadas `[TEST]` no
prefixo descritivo e podem ser podadas pelo time se a estratégia de
validação mudar.

**Organization**: Tarefas agrupadas por user story para permitir
implementação e validação independentes. Dependências explícitas em
cada item.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo com outras [P] (arquivos distintos, sem
  dependência de tarefas incompletas).
- **[Story]**: US1, US2, US3, US4 (mapeia para as user stories do
  `spec.md`).
- Caminhos absolutos no repo: `C:\repos\ProxyPay\ProxyPay\...`.

## Path Conventions

Backend .NET Clean Architecture (já existente):
- API: `C:\repos\ProxyPay\ProxyPay\ProxyPay.API\`
- Application: `C:\repos\ProxyPay\ProxyPay\ProxyPay.Application\`
- Domain: `C:\repos\ProxyPay\ProxyPay\ProxyPay.Domain\`
- Infra: `C:\repos\ProxyPay\ProxyPay\ProxyPay.Infra\`
- Tests: `C:\repos\ProxyPay\ProxyPay\ProxyPay.Tests\`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar configuração local de dois tenants distintos e
ajustar appsettings em todos os ambientes para o alvo desta feature.

- [ ] T001 Criar os bancos PostgreSQL locais `proxypay_emagine_dev` e `proxypay_fortuno_dev` via psql (passo documentado em `specs/001-multi-tenant-fortuno/quickstart.md` seção 1) — **PENDENTE (requer psql local do usuário)**
- [X] T002 [P] `appsettings.Development.json` atualizado — **CORRIGIDO 2026-04-17**: descoberta tardia de que `monexup` é tenant legítimo pré-existente (config em `.env` / `docker-compose.yml` / DB DO dedicado `monexup`), não algo a remover. Estado final: três blocos de tenant em appsettings (`emagine` → `proxypay_emagine_dev`, `monexup` → `proxypay_monexup_dev`, `fortuno` → `proxypay_fortuno_dev`), JwtSecrets distintos por tenant (emagine+monexup alinhados com `.env`). **Chaves AbacatePay mantidas globais** até T025/T037 completarem o move per-tenant.
- [X] T003 [P] `appsettings.Docker.json` atualizado — três blocos de tenant com strings vazias (preenchidas via env vars pelo docker-compose).
- [X] T004 [P] `appsettings.Production.json` atualizado — três blocos com `BucketName` distinto (`proxypay-emagine`, `proxypay-monexup`, `proxypay-fortuno`); CS/JwtSecret vazios (env vars).
- [X] T002a [P] **NOVA (não planejada originalmente)**: arquivos de env e docker-compose reconciliados com os 3 tenants: `.env`, `.env.example`, `.env.prod`, `.env.prod.example` ganharam bloco `FORTUNO_*`; `docker-compose.yml` e `docker-compose-prod.yml` ganharam mapeamento `Tenants__fortuno__*`. `.env.prod` tem o `FORTUNO_CONNECTION_STRING` apontando para DB `fortuno` no cluster DO (ainda a criar) e `FORTUNO_JWT_SECRET=` vazio (gerar antes do deploy).
- [ ] T005 Aplicar as 5 migrations existentes aos dois bancos dev via `dotnet ef database update --project ProxyPay.Infra --startup-project ProxyPay.API --connection "<cs-emagine>"` e repetir para fortuno (fallback manual até o runner ficar pronto em T035) — **PENDENTE (requer dotnet-ef + Postgres local)**

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Introduzir o catálogo de tenants e a capacidade de definir
tenant fora do HTTP context — ambos são usados por todas as user stories
subsequentes.

**⚠️ CRITICAL**: Nenhuma user story pode começar antes desta phase
completar.

- [X] T006 `ITenantCatalog` criado em `ProxyPay.Domain\Interfaces\ITenantCatalog.cs` com `GetActiveTenantIds()`, `IsKnownTenant(string)` e `UnknownTenantException`.
- [X] T007 `TenantCatalog : ITenantCatalog` criado em `ProxyPay.Application\TenantCatalog.cs` lendo seção `Tenants` de `IConfiguration`. Nota: implementação atual lê direto do `IConfiguration`, sem cache — aceitável porque `IConfiguration` já cacheia internamente; `IOptionsMonitor` fica para quando reload dinâmico for testado em T032.
- [X] T008 `ITenantCatalog` registrado como `Singleton` em `ProxyPay.Application\Startup.cs` dentro de `#region Tenant`.
- [X] T009 Invariante de startup implementada em `ProxyPay.API\Program.cs` como método `EnforceTenantIsolationInvariants`, chamado após `Build()` e antes de `host.Run()`. Lança `InvalidOperationException` se duas connection strings forem idênticas.
- [X] T010 `TenantContext.cs` agora expõe `static IDisposable EnterScope(string tenantId)` via `AsyncLocal<string>`. `TenantId` retorna `AsyncLocal.Value ?? HttpContextItem ?? null`.
- [X] T011 Fallback silencioso removido de `TenantResolver.cs`. Quando `_tenantContext.TenantId` é null, lança `InvalidOperationException` com mensagem explicativa direcionando a `X-Tenant-Id` header ou `TenantContext.EnterScope(...)`.

**Checkpoint**: Foundation pronta — user stories podem avançar em paralelo.

---

## Phase 3: User Story 1 - Isolamento total de dados entre tenants (Priority: P1) 🎯 MVP

**Goal**: Garantir que qualquer dado persistido ou lido por uma requisição
esteja confinado ao banco físico do tenant daquela requisição, sem
qualquer vazamento cross-tenant.

**Independent Test**: Povoar `emagine` e `fortuno` com stores distintas,
autenticar em cada um e confirmar que listagens, gets por ID e tentativas
de modificação nunca retornam ou afetam dados do outro tenant.

### Tests for User Story 1

- [X] T012 [P] [US1] [TEST] `CrossTenantIsolationTests.cs` criado em `ProxyPay.Tests\Integration\` — **ESQUELETO**: 4 `[Fact(Skip=...)]` com cenários documentados em Arrange/Act/Assert. Infraestrutura de integração (`WebApplicationFactory` + fixtures de 2 bancos reais + seeds) **ainda não existe** no `ProxyPay.Tests` — ativar os testes exige adicionar essa infra primeiro (cabeçalho do arquivo detalha).
- [X] T013 [P] [US1] [TEST] Cenário `CrossTenantInvoiceIdCollision_ResolvesToCorrectTenant` incluído no mesmo arquivo de T012 como 4º `[Fact(Skip=...)]`.

### Implementation for User Story 1

- [X] T014 [US1] Auditoria PASSOU — os 7 repositórios em `ProxyPay.Infra\Repository\` (Invoice, Transaction, Customer, Store, Billing, BillingItem, InvoiceItem) injetam `ProxyPayContext` via construtor. Grep confirmou zero `new ProxyPayContext`, zero `NpgsqlConnection`, zero hardcoded connection string.
- [X] T015 [US1] `AbacatePayAppService` é cliente HTTP puro (sem DbContext), portanto não viola o critério da task. **Achados laterais documentados (fora do escopo US1)**: (a) `new HttpClient` manual ignora IHttpClientFactory + `TenantHeaderHandler` — será endereçado em T025; (b) `ServerCertificateCustomValidationCallback = ... => true` é dívida pré-existente de TLS.
- [X] T016 [US1] Comentário inline adicionado no registro Scoped do `ProxyPayContext` em `ProxyPay.Application\Startup.cs` — explica por que não pode ser Singleton (cross-tenant bleed) nem Transient (fragmentação de transação).
- [ ] T017 [US1] Rodar `dotnet test` executando T012 e T013 — **PENDENTE (esqueletos [Skip]; testes não rodam até infra de integração existir)**

**Checkpoint**: Isolamento físico validado; cross-tenant por ID retorna 404.

---

## Phase 4: User Story 2 - Roteamento automático por tenant em toda requisição (Priority: P1)

**Goal**: Toda requisição HTTP resolve automaticamente o tenant via
header `X-Tenant-Id` (ou path em webhooks), rejeita ausências/desconhecidos
antes de qualquer acesso a dados, e propaga o tenant para chamadas outbound.

**Independent Test**: Enviar a mesma requisição com credenciais de dois
tenants diferentes e validar respostas escopadas corretamente; enviar sem
header e validar 400; chamar um AppService que faz HTTP outbound e
inspecionar que o header `X-Tenant-Id` saiu no request.

### Tests for User Story 2

- [ ] T018 [P] [US2] [TEST] Criar `TenantMiddlewareTests.cs` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.Tests\Integration\` cobrindo: (a) `POST /graphql` sem `X-Tenant-Id` → 400 com body `{"error":"tenant_context_required"}`, (b) `POST /graphql` com `X-Tenant-Id: inexistente` → 400 mesma resposta, (c) `GET /` sem header → 200 (exempção de health check), (d) `POST /graphql` com `X-Tenant-Id: emagine` → 200
- [ ] T019 [P] [US2] [TEST] Criar `TenantHeaderHandlerTests.cs` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.Tests\Unit\` que usa `HttpMessageInvoker` com um handler mock downstream, simula uma requisição com `TenantContext.TenantId = "fortuno"` e valida que o outbound request tem header `X-Tenant-Id: fortuno`
- [ ] T020 [P] [US2] [TEST] Criar `PaymentControllerTenantIsolationTest` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.Tests\Integration\` que semeia uma Store com `ClientId="X"` em `emagine`, depois chama `POST /payment/billing` com `X-Tenant-Id: fortuno` e `ClientId="X"` no body — deve responder erro (store não encontrado em `fortuno`, não expõe a store de `emagine`)

### Implementation for User Story 2

- [ ] T021 [US2] Reescrever `C:\repos\ProxyPay\ProxyPay\ProxyPay.API\Middlewares\TenantMiddleware.cs`: (a) injetar `ITenantCatalog` via construtor; (b) exemption list configurável (`GET /`, `GET /swagger/*`); (c) quando path começa com `/webhook/abacatepay/`, extrair tenant do próximo segmento e validar via `ITenantCatalog.IsKnownTenant`; (d) caso geral: ler header `X-Tenant-Id`, validar no catálogo; (e) em qualquer rejeição (ausente, vazio, desconhecido) responder `400` com body `{"error":"tenant_context_required"}` e encerrar pipeline; (f) em sucesso, setar `HttpContext.Items["TenantId"]`
- [ ] T022 [US2] Criar `TenantHeaderHandler : DelegatingHandler` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.Application\TenantHeaderHandler.cs` que injeta `X-Tenant-Id: {ITenantContext.TenantId}` no request outbound; se `TenantId` for null (chamada fora de request — ex.: runner), não adiciona header e loga warning via `ILogger<TenantHeaderHandler>`
- [ ] T023 [US2] Registrar `TenantHeaderHandler` como `Scoped` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.Application\Startup.cs` dentro de `#region Tenant`
- [ ] T024 [US2] Alterar `C:\repos\ProxyPay\ProxyPay\ProxyPay.Application\Startup.cs` região `#region Client`: substituir cada `injectDependency(typeof(IUserClient), typeof(UserClient), ...)` pela forma tipada `services.AddHttpClient<IUserClient, UserClient>().AddHttpMessageHandler<TenantHeaderHandler>()` (e análogos para `IChatGPTClient`, `IMailClient`, `IFileClient`, `IStringClient`, `IDocumentClient`) — garantir que todos os clients externos propaguem o header
- [ ] T025 [US2] Alterar `C:\repos\ProxyPay\ProxyPay\ProxyPay.Infra\AppServices\AbacatePayAppService.cs` (ou onde estiver o HttpClient da AbacatePay) para também passar pelo `TenantHeaderHandler` ou receber `HttpClient` de `IHttpClientFactory` com o handler anexado
- [ ] T026 [US2] Revisar `C:\repos\ProxyPay\ProxyPay\ProxyPay.API\Controllers\PaymentController.cs` — confirmar que `IStoreService.GetByClientIdAsync(billing.ClientId)` usa o `ProxyPayContext` já escopado ao tenant corrente (via `TenantDbContextFactory`); **não** fazer alteração estrutural no controller, apenas garantir via audit que nenhuma busca bypassa o scope
- [ ] T027 [US2] Rodar `dotnet test` executando T018, T019 e T020; todos devem passar

**Checkpoint**: Middleware enforcement ativo; header propagado outbound;
PaymentController fica naturalmente seguro por escopar DbContext.

---

## Phase 5: User Story 3 - Credenciais de autenticação isoladas por tenant (Priority: P1)

**Goal**: Segredos JWT por tenant já presentes no catálogo; validar que
NAuth realmente os usa dinamicamente e que rotação é possível sem affect
outros tenants.

**Independent Test**: Emitir token em `emagine`, apresentar em `fortuno`
→ 401 indistinguível de token expirado; rotacionar segredo de um tenant
e verificar que tokens do outro continuam válidos.

### Tests for User Story 3

- [ ] T028 [P] [US3] [TEST] Criar `CrossTenantTokenTests.cs` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.Tests\Integration\` que: (a) emite token usando `Tenants:emagine:JwtSecret`, apresenta-o em `POST /store/insert` com `X-Tenant-Id: fortuno` → 401; (b) emite token expirado (exp no passado) e apresenta no tenant correto → 401 com body **idêntico byte-a-byte** ao caso (a); (c) emite token assinado com secret aleatório desconhecido → 401 idêntico
- [ ] T029 [P] [US3] [TEST] Criar `JwtSecretRotationTest` no mesmo arquivo que simula rotação de `Tenants:emagine:JwtSecret` (via `IConfigurationRoot.Reload()` ou recriação do host de teste), assegura que um token emitido antes da rotação é rejeitado em `emagine`, e que tokens de `fortuno` (não rotacionado) continuam válidos

### Implementation for User Story 3

- [ ] T030 [US3] Auditar `C:\repos\ProxyPay\ProxyPay\ProxyPay.Application\NAuthTenantSecretProvider.cs` — confirmar que `GetJwtSecret(tenantId)` lê via `_configuration[$"Tenants:{tenantId}:JwtSecret"]` dinamicamente a cada chamada (não cacheia em field) e não loga o valor do segredo (apenas o `tenantId`)
- [ ] T031 [US3] Revisar `C:\repos\ProxyPay\ProxyPay\ProxyPay.API\Filters\GlobalExceptionFilter.cs` — garantir que respostas 401/403 **não** incluem `ex.Message` nem stack trace que possa vazar diferenciação entre "tenant desconhecido", "segredo inválido", "token expirado"; padronizar para retornar `401 Unauthorized` com body fixo `{"error":"unauthorized"}` nos três casos
- [ ] T032 [US3] Habilitar reload de `IConfiguration` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.API\Program.cs`: `AddJsonFile("appsettings.json", optional:false, reloadOnChange:true)` já é o default do `WebApplication.CreateBuilder` — apenas adicionar um teste manual no `quickstart.md` troubleshooting confirmando que editar `JwtSecret` em dev invalida tokens antigos sem precisar reiniciar a API
- [ ] T033 [US3] Rodar `dotnet test` executando T028 e T029; ambos devem passar

**Checkpoint**: Isolamento de credenciais validado; rotação por tenant
funciona sem downtime cross-tenant.

---

## Phase 6: User Story 4 - Provisionamento de "emagine" e "fortuno" em produção (Priority: P2)

**Goal**: Entregar ferramenta de migrations cross-tenant, rota de webhook
por tenant, e o runbook executável para promover a feature em produção.

**Independent Test**: Executar o runbook de produção em ambiente de
staging, completar o fluxo fim-a-fim (login → store → invoice → webhook)
nos dois tenants e confirmar persistência correta em cada banco físico.

### Tests for User Story 4

- [ ] T034 [P] [US4] [TEST] Criar `WebhookPerTenantRoutingTests.cs` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.Tests\Integration\` que: (a) chama `POST /webhook/abacatepay/fortuno?secret=<secret-fortuno>` com payload válido → 200 e `Invoice` de `fortuno` atualizado; (b) chama `POST /webhook/abacatepay/fortuno?secret=<secret-emagine>` → 200 mas log de warning e nenhuma atualização; (c) chama `POST /webhook/abacatepay/inexistente?secret=qualquer` → 404

### Implementation for User Story 4

- [ ] T035 [US4] Criar `MigrationRunner.cs` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.API\Tools\MigrationRunner.cs`: classe com método `Task<int> RunAsync(string[] args)` que (a) resolve `ITenantCatalog` e `TenantDbContextFactory`, (b) para cada tenant, abre `TenantContext.EnterScope(tenantId)` e chama `factory.CreateDbContext().Database.MigrateAsync()`, (c) imprime log estruturado por tenant (id, migrations aplicadas, duração), (d) retorna código 0 em sucesso total ou código não-zero com mensagem no primeiro erro
- [ ] T036 [US4] Alterar `C:\repos\ProxyPay\ProxyPay\ProxyPay.API\Program.cs` para detectar o flag `--migrate-all-tenants` nos `args` antes de `app.Run()`, executar `MigrationRunner.RunAsync(args)` e sair com o código retornado sem iniciar o host HTTP
- [ ] T037 [US4] Mover a validação `string secret` do `AbacatePay:WebhookSecret` global para leitura via `Tenants:{tenantCorrente}:AbacatePayWebhookSecret` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.API\Controllers\WebhookController.cs`; acessar através de `ITenantResolver` injetado no construtor
- [ ] T038 [US4] Alterar rota do `WebhookController.AbacatePayWebhook` em `C:\repos\ProxyPay\ProxyPay\ProxyPay.API\Controllers\WebhookController.cs` de `[HttpPost("abacatepay")]` para `[HttpPost("abacatepay/{tenantId}")]`; o parâmetro `tenantId` do método serve apenas como referência — a resolução já foi feita pelo `TenantMiddleware` (T021) via path segment
- [ ] T039 [US4] Rodar `dotnet test` executando T034; deve passar
- [ ] T040 [US4] Validar end-to-end local seguindo `specs/001-multi-tenant-fortuno/quickstart.md` seções 4 e 5: subir API, executar smoke tests 5.1–5.6, registrar evidência textual (output dos comandos) em comentário no PR

**Checkpoint**: Runner de migrations operacional; webhook por tenant
funcional; quickstart validado.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Fechar ruídos remanescentes sem alterar comportamento
funcional.

- [ ] T041 [P] Atualizar `C:\repos\ProxyPay\ProxyPay\CLAUDE.md` seção "Architecture" para refletir o domínio real (stores, customers, invoices, billings, transactions — não products/orders/categories que não existem no código); mencionar explicitamente que multi-tenancy é completa após esta feature (dois tenants ativos em produção)
- [ ] T042 [P] Criar nota em `C:\repos\ProxyPay\ProxyPay\specs\001-multi-tenant-fortuno\research.md` (fim do arquivo) registrando como **follow-up fora de escopo** o ajuste de CORS `AllowAnyOrigin` em `ProxyPay.API\Startup.cs` (violação pré-existente do Princípio V da constituição — vide R-010)
- [ ] T043 Revisar todos os arquivos alterados nesta feature contra a constituição `.specify\memory\constitution.md`: conferir file-scoped namespaces, PascalCase, `[JsonPropertyName("camelCase")]` em DTOs (se houver); corrigir desvios; zero resultados para `grep -n "namespace .* {"` (forma bloco) nos arquivos novos
- [ ] T044 Executar `dotnet build ProxyPay.sln` e `dotnet test ProxyPay.Tests` da raiz; ambos devem passar com zero warnings introduzidos por esta feature

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: sem dependências — pode iniciar imediatamente.
- **Phase 2 (Foundational)**: depende de Phase 1 completa. **Bloqueia
  todas as user stories.**
- **Phase 3 (US1)**: depende de Phase 2. Independente das demais US.
- **Phase 4 (US2)**: depende de Phase 2. Independente das demais US; toca
  `TenantMiddleware` e DI de clients — evitar merge simultâneo com
  Phase 5 em files compartilhados (`Startup.cs` da Application).
- **Phase 5 (US3)**: depende de Phase 2. Independente das demais US.
- **Phase 6 (US4)**: depende de Phase 2 e **de Phase 4** (porque o runner
  usa `TenantContext.EnterScope` introduzido em T010 e o webhook usa o
  middleware reescrito em T021).
- **Phase 7 (Polish)**: depende de todas as user stories desejadas.

### Task-level dependencies críticas

- T011 depende de T010 (EnterScope precisa existir antes de remover o fallback).
- T021 depende de T006+T007+T008 (middleware precisa do catálogo).
- T024 depende de T022+T023 (registro do handler antes de anexar a clients).
- T027, T017, T033, T039 dependem dos testes e da implementação do respectivo phase.
- T035 depende de T010 (EnterScope) + T008 (catalog).
- T036 depende de T035.
- T037/T038 dependem de T021 (middleware precisa reconhecer path webhook).

### Parallel Opportunities

- **Setup**: T002, T003, T004 são 3 arquivos distintos → podem rodar em paralelo.
- **Foundational**: T006 (Domain) e T007 (Application) são arquivos distintos; T007 depende da interface de T006 estar commitada antes do compile final, mas os arquivos podem ser editados em paralelo e commitados juntos.
- **US1**: T012 e T013 no mesmo arquivo de teste (sequencial dentro do arquivo, mas paralelo com T014/T015 que são audits em outros arquivos).
- **US2**: T018, T019, T020 são 3 arquivos de teste distintos → paralelos.
- **US3**: T028 e T029 no mesmo arquivo (sequenciais) mas paralelos com T030 (audit de outro arquivo).
- **US4**: T034 isolado; T035 e T037/T038 tocam arquivos distintos → paralelos.
- **Polish**: T041 e T042 paralelos (arquivos distintos).

### Within Each User Story

- Testes (T012/T013, T018-T020, T028-T029, T034): escritos antes da
  implementação, devem falhar antes do fix e passar depois.
- Modificações de código: middleware → DI → clients → controller (US2);
  audit → filter → config (US3); runner → Program.cs → webhook (US4).

---

## Parallel Example: User Story 2

```bash
# Primeira onda — 3 arquivos de teste distintos, simultâneos:
Task T018: "TenantMiddlewareTests.cs em ProxyPay.Tests/Integration/"
Task T019: "TenantHeaderHandlerTests.cs em ProxyPay.Tests/Unit/"
Task T020: "PaymentControllerTenantIsolationTest em ProxyPay.Tests/Integration/"

# Segunda onda — após T021 ser mergeado:
Task T022: "TenantHeaderHandler.cs em ProxyPay.Application/"
Task T026: "Audit PaymentController.cs em ProxyPay.API/Controllers/"

# Terceira onda — depende de T022:
Task T024: "Alterar Startup.cs para registrar handler nos clients"
Task T025: "Alterar AbacatePayAppService.cs"
```

---

## Implementation Strategy

### MVP First (Phase 1 → Phase 2 → Phase 3 — US1)

Entregar isolamento de dados validado (SC-001). Essa é a entrega mínima
útil — a feature só tem sentido se o isolamento funciona. Deploy em Dev
após T017 passar.

### Incremental Delivery

1. **MVP**: Setup + Foundational + US1 → validar isolamento em Dev.
2. **+ US2**: enforcement sem fallback + propagação outbound → ambientes
   Dev/Staging.
3. **+ US3**: auditoria de credenciais + rotação testada.
4. **+ US4**: runner de migrations + webhook por tenant + runbook de
   produção → pronto para deploy real.
5. **Polish**: documentação (CLAUDE.md) + follow-up de CORS (em PR separada).

### Parallel Team Strategy

Com múltiplos desenvolvedores, após Foundational (Phase 2):

- **Dev A**: US1 (isolation audit + tests) — arquivos de repositório.
- **Dev B**: US2 (middleware + handler + DI) — arquivos de Application/API.
- **Dev C**: US3 (credential audit + filter hardening) — arquivo de
  Filters/NAuthTenantSecretProvider.
- Ao final, **Dev B ou C** assume US4 (depende de pieces de US2).

---

## Notes

- Tarefas [P] indicam arquivos distintos e independência de incompletas.
- Todas as user stories são independentemente testáveis conforme
  critério da spec.
- Commit após cada tarefa ou grupo lógico (ex.: um commit por
  [TEST]+implementation de um requisito).
- Evitar: vagueza em paths, conflitos de merge em `Startup.cs`
  (`ProxyPay.Application\Startup.cs` é tocado por T008, T023 e T024 —
  coordenar ordem).
- Nenhum dos testes deve mockar o DbContext — integração real contra
  bancos locais `proxypay_emagine_dev` e `proxypay_fortuno_dev`
  (conforme constituição: sem Docker local).
