---
description: "Task list for 003-api-endpoint-tests"
---

# Tasks: Testes de API para todos os endpoints existentes

**Input**: Design documents from `C:\repos\ProxyPay\specs\003-api-endpoint-tests\`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api-test-matrix.md, quickstart.md

**Tests**: Este projeto **é** uma suíte de testes — os "testes" são o próprio entregável. Cada fase de user story produz uma classe de teste por área de endpoint (não há código de produção a testar à parte). Não se aplica a regra TDD de "falhar antes" no sentido usual; a ordem é fixture → classes de teste por área.

**Organization**: Tarefas agrupadas por user story (área de endpoint) conforme spec.md, para permitir implementação e validação independentes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências pendentes)
- **[Story]**: US1=Payment, US2=Store, US3=Webhook, US4=GraphQL
- Caminhos relativos à raiz da solution `backend/`

## Path Conventions

Projeto único de testes: `backend/ProxyPay.ApiTests/` (irmão de `backend/ProxyPay.Tests/`), conforme a skill `dotnet-test-api` e o `plan.md`. Namespaces **file-scoped** (constituição §III).

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Criar e configurar o projeto `ProxyPay.ApiTests` via o boot flow da skill `dotnet-test-api`.

- [x] T001 Criar o projeto xUnit em `backend/ProxyPay.ApiTests` com `dotnet new xunit -n ProxyPay.ApiTests -o ProxyPay.ApiTests` (executar a partir de `backend/`)
- [x] T002 Adicionar o projeto à solution: `dotnet sln ProxyPay.sln add ProxyPay.ApiTests/ProxyPay.ApiTests.csproj` (a partir de `backend/`)
- [x] T003 Adicionar os pacotes NuGet com versões mínimas (xunit 2.5.3, xunit.runner.visualstudio 2.5.3, Microsoft.NET.Test.Sdk 17.8.0, FluentAssertions 7.0.0, Flurl.Http 4.0.2, Microsoft.Extensions.Configuration 9.0.8, Microsoft.Extensions.Configuration.Json 9.0.8, Microsoft.Extensions.Configuration.EnvironmentVariables 9.0.8, coverlet.collector 6.0.0) em `backend/ProxyPay.ApiTests/ProxyPay.ApiTests.csproj`
- [x] T004 Adicionar referência ao único projeto DTO: `dotnet add ProxyPay.ApiTests/ProxyPay.ApiTests.csproj reference ProxyPay.DTO/ProxyPay.DTO.csproj` (não referenciar Domain/Application/Infra/API)
- [x] T005 [P] Editar `backend/ProxyPay.ApiTests/ProxyPay.ApiTests.csproj` para incluir `<Using Include="Xunit" />` e o bloco `<Content Include="appsettings.Test.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>`
- [x] T006 [P] Criar `backend/ProxyPay.ApiTests/appsettings.Test.json` com placeholders `REPLACE_VIA_ENV_*` para todas as chaves de `data-model.md` (`ApiBaseUrl`, `Auth:BaseUrl/Email/Password/Tenant/LoginEndpoint`, `Store:ClientId`, `Webhook:Secret`, `Timeout`)
- [x] T007 Remover o arquivo de teste de exemplo gerado pelo template (`backend/ProxyPay.ApiTests/UnitTest1.cs`)

**Checkpoint**: Projeto compila vazio (`dotnet build ProxyPay.ApiTests/`).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Fixture compartilhada de autenticação NAuth Basic + tenant. **Bloqueia todas as user stories** (todas dependem da fixture).

**⚠️ CRITICAL**: Nenhuma classe de teste pode ser escrita antes desta fase.

- [x] T008 Criar `backend/ProxyPay.ApiTests/Fixtures/ApiTestFixture.cs` (`IAsyncLifetime`, namespace file-scoped): carregar config (`appsettings.Test.json` + env vars), método `RequireConfig` com fast-fail em placeholders `REPLACE_VIA_ENV_*`, login único NAuth via `Auth:BaseUrl` + `Auth:LoginEndpoint` (default `/user/loginWithEmail`) com `X-Tenant-Id`, e propriedades `BaseUrl`/`AuthToken`/`Tenant`
- [x] T009 Em `ApiTestFixture.cs`, implementar `CreateAuthenticatedRequest(path)` aplicando `Authorization: Basic {token}` **+** `X-Tenant-Id`, e `CreateAnonymousRequest(path)` com `X-Tenant-Id` sem `Authorization` (adaptação NAuth Basic, ver research.md R1)
- [x] T010 [P] Criar `backend/ProxyPay.ApiTests/Fixtures/ApiTestCollection.cs` com `[CollectionDefinition("ApiTests")]` → `ICollectionFixture<ApiTestFixture>`
- [x] T011 [P] Criar shell vazio `backend/ProxyPay.ApiTests/Helpers/TestDataHelper.cs` (classe estática, namespace file-scoped) para as factories de payload crescerem por story
- [x] T012 [P] Criar shell `backend/ProxyPay.ApiTests/Helpers/GraphQLQueries.cs` (classe estática) para as constantes string das consultas GraphQL
- [x] T013 Build de verificação: `dotnet build ProxyPay.ApiTests/` compila com a fixture e os helpers vazios

**Checkpoint**: Fundação pronta — as user stories podem começar.

---

## Phase 3: User Story 1 - Endpoints de pagamento (Priority: P1) 🎯 MVP

**Goal**: Cobrir `POST /payment/billing`, `POST /payment/invoice`, `POST /payment/qrcode`, `GET /payment/qrcode/status/{id}`, `POST /payment/simulate-payment/{id}` (sucesso + validação).

**Independent Test**: Com a API + sandbox AbacatePay rodando e `Store:ClientId` válido, `dotnet test --filter PaymentControllerTests` passa (sucesso e `400` de validação) sem depender das outras stories.

- [x] T014 [US1] Adicionar factories a `backend/ProxyPay.ApiTests/Helpers/TestDataHelper.cs`: `CreateBillingRequest(...)`, `CreateInvoiceRequest(...)`, `CreateQRCodeRequest(...)`, `CreateCustomerInsertInfo(...)` (defaults válidos; sobrecargas para customer nulo / sem email), usando os DTOs de `ProxyPay.DTO.Billing/Invoice/Customer`
- [x] T015 [US1] Criar `backend/ProxyPay.ApiTests/Controllers/PaymentControllerTests.cs` (`[Collection("ApiTests")]`, namespace file-scoped) com os testes de billing: `CreateBilling_WithValidBody_ShouldReturnOk`, `CreateBilling_WithoutCustomer_ShouldReturn400`, `CreateBilling_WithoutCustomerEmail_ShouldReturn400`
- [x] T016 [US1] Adicionar a `PaymentControllerTests.cs` os testes de invoice e qrcode: `CreateInvoice_WithValidBody_ShouldReturnOk`, `CreateInvoice_WithoutCustomer_ShouldReturn400`, `CreateQRCode_WithValidBody_ShouldReturnOk`, `CreateQRCode_WithoutCustomerEmail_ShouldReturn400`
- [x] T017 [US1] Adicionar a `PaymentControllerTests.cs` os testes de status e simulação (sandbox): `CheckQRCodeStatus_ForExistingInvoice_ShouldReturnOk`, `CheckQRCodeStatus_ForUnknownInvoice_ShouldReturn400`, `SimulatePayment_ForExistingInvoice_ShouldReturnOk` (criar a fatura no próprio fluxo do teste — idempotência R5)

**Checkpoint**: US1 (MVP) totalmente funcional e testável de forma independente.

---

## Phase 4: User Story 2 - Endpoints de loja e autorização (Priority: P2)

**Goal**: Cobrir `POST /store`, `PUT /store`, `PUT /store/{id}/abacatepay-apikey`, `DELETE /store/{id}` (sucesso, `401` anônimo, `403` recurso de terceiro).

**Independent Test**: `dotnet test --filter StoreControllerTests` passa — criação `201`, `401` sem auth e `403` para store não-própria — sem depender das outras stories.

- [x] T018 [P] [US2] Adicionar factories a `backend/ProxyPay.ApiTests/Helpers/TestDataHelper.cs`: `CreateStoreInsertInfo(...)`, `CreateStoreUpdateInfo(...)`, `CreateStoreApiKeyUpdateInfo(...)` usando DTOs de `ProxyPay.DTO.Store`
- [x] T019 [US2] Criar `backend/ProxyPay.ApiTests/Controllers/StoreControllerTests.cs` (`[Collection("ApiTests")]`, file-scoped) com os testes anônimos `401`: `Create_WithoutAuth_ShouldReturn401`, `Update_WithoutAuth_ShouldReturn401`, `SetAbacatePayApiKey_WithoutAuth_ShouldReturn401`, `Delete_WithoutAuth_ShouldReturn401`
- [x] T020 [US2] Adicionar a `StoreControllerTests.cs` o happy-path de criação `Create_WithValidBody_ShouldReturn201` (afirmar contrato: `201` + `storeId`/`clientId`; sem deletar — idempotência R5)
- [x] T021 [US2] Adicionar a `StoreControllerTests.cs` os testes de autorização de recurso `403`: `Update_ForNonOwnedStore_ShouldReturn403`, `SetAbacatePayApiKey_ForNonOwnedStore_ShouldReturn403`, `Delete_ForNonOwnedStore_ShouldReturn403`, e o happy-path `SetAbacatePayApiKey_WithValidBody_ShouldReturn204` sobre uma store própria criada no fluxo

**Checkpoint**: US1 e US2 funcionam de forma independente.

---

## Phase 5: User Story 3 - Webhook e proteções (Priority: P3)

**Goal**: Cobrir `POST /webhook/abacatepay` (segredo ausente/inválido/válido, payload completo/incompleto), sempre `200` por design (anti-retry §6).

**Independent Test**: `dotnet test --filter WebhookControllerTests` passa — todos retornam `200`; com segredo válido e payload completo, o evento é processado — usando `Webhook:Secret` da config.

- [x] T022 [P] [US3] Adicionar factory a `backend/ProxyPay.ApiTests/Helpers/TestDataHelper.cs`: `CreateAbacatePayWebhookPayload(...)` (com/sem `data`, `event`, `devMode`) usando `ProxyPay.DTO.AbacatePay`
- [x] T023 [US3] Criar `backend/ProxyPay.ApiTests/Controllers/WebhookControllerTests.cs` (`[Collection("ApiTests")]`, file-scoped) com: `Webhook_WithoutSecret_ShouldReturn200AndIgnore`, `Webhook_WithInvalidSecret_ShouldReturn200AndIgnore`, `Webhook_WithValidSecretMissingData_ShouldReturn200`, `Webhook_WithValidSecretAndPayload_ShouldReturn200` (compor a query `secret` com `SetQueryParam`)

**Checkpoint**: US1, US2 e US3 funcionam de forma independente.

---

## Phase 6: User Story 4 - Consultas GraphQL autenticadas (Priority: P3)

**Goal**: Cobrir `POST /graphql` (myStores, myInvoices, myInvoiceByNumber, myTransactions, myBalance, myCustomers) e o stub `POST /api/graphql-docs` (`[Authorize]`).

**Independent Test**: `dotnet test --filter GraphQLControllerTests` passa — consultas autenticadas retornam `data` sem `errors`; anônima é rejeitada; stub docs dá `401` anônimo e `200` autenticado.

- [x] T024 [P] [US4] Preencher `backend/ProxyPay.ApiTests/Helpers/GraphQLQueries.cs` com as constantes das 6 consultas (`MyStores`, `MyInvoices`, `MyInvoiceByNumber`, `MyTransactions`, `MyBalance`, `MyCustomers`) conforme `ProxyPay.GraphQL/Admin/AdminQuery.cs`
- [x] T025 [US4] Criar `backend/ProxyPay.ApiTests/Controllers/GraphQLControllerTests.cs` (`[Collection("ApiTests")]`, file-scoped) com os testes autenticados das 6 consultas (`Graphql_<Query>_WithAuth_ShouldReturnData`, afirmando `200` + `data` presente e sem `errors`) postando `GraphQLRequest` em `/graphql`
- [x] T026 [US4] Adicionar a `GraphQLControllerTests.cs`: `Graphql_MyStores_WithoutAuth_ShouldBeUnauthorized` (anônimo em `/graphql`) e os testes do stub `GraphqlDocs_WithoutAuth_ShouldReturn401`, `GraphqlDocs_WithAuth_ShouldReturn200` em `/api/graphql-docs`

**Checkpoint**: Todas as user stories funcionam de forma independente.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validação final e fechamento.

- [x] T027 Build completo da suíte: `dotnet build ProxyPay.ApiTests/` sem erros nem warnings de namespace
- [x] T028 Verificar que `Helpers/TestDataHelper.cs` não tem factories órfãs (toda factory referenciada por ≥1 classe de teste) e que todos os métodos seguem `<Method>_<Condition>_ShouldReturn<Expected>`
- [x] T029 Executar a validação do `quickstart.md`: exportar as env vars, `dotnet test ProxyPay.ApiTests/` com a API + NAuth + sandbox em execução, e confirmar repetibilidade rodando duas vezes (SC-006)
- [x] T030 [P] Emitir o bloco "Como fornecer segredos" (lista de cada placeholder `REPLACE_VIA_ENV_*` e o env var correspondente para bash/PowerShell/GitHub Actions) — anexar ao final de `quickstart.md` se ainda não coberto

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — começa imediatamente. T001→T002→T003→T004 são sequenciais; T005/T006 [P] após T001; T007 após T001.
- **Foundational (Phase 2)**: Depende do Setup. **BLOQUEIA todas as user stories.** T008→T009 sequenciais (mesmo arquivo); T010/T011/T012 [P] após T008; T013 fecha a fase.
- **User Stories (Phase 3–6)**: Todas dependem do Phase 2. Podem rodar em paralelo (arquivos de teste distintos), respeitando a ordem de prioridade P1→P2→P3 se sequencial.
- **Polish (Phase 7)**: Depende das user stories desejadas concluídas.

### User Story Dependencies

- **US1 Payment (P1)**: Após Foundational — independente. MVP.
- **US2 Store (P2)**: Após Foundational — independente.
- **US3 Webhook (P3)**: Após Foundational — independente.
- **US4 GraphQL (P3)**: Após Foundational — independente.

> Acoplamento de arquivo: as tarefas que **adicionam factories ao mesmo `TestDataHelper.cs`** (T014, T018, T022) não são mutuamente [P] se executadas concorrentemente; estão marcadas [P] apenas quando a story roda isolada. As classes `Controllers/*Tests.cs` são arquivos distintos e plenamente paralelizáveis entre stories.

### Within Each User Story

- Factories (TestDataHelper) antes da classe de teste que as usa.
- GraphQLQueries (T024) antes de `GraphQLControllerTests` (T025/T026).

### Parallel Opportunities

- Setup: T005, T006 em paralelo (após T001).
- Foundational: T010, T011, T012 em paralelo (após T008).
- Entre stories (após Phase 2): as quatro classes `Controllers/*Tests.cs` podem ser escritas em paralelo por pessoas diferentes.

---

## Parallel Example: após a Fundação (Phase 2)

```bash
# Quatro classes de teste em arquivos distintos, em paralelo:
Task: "PaymentControllerTests.cs (US1)"
Task: "StoreControllerTests.cs (US2)"
Task: "WebhookControllerTests.cs (US3)"
Task: "GraphQLControllerTests.cs (US4)"
# Cuidado: a adição de factories ao TestDataHelper.cs compartilhado deve ser serializada.
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Phase 1 (Setup) → 2. Phase 2 (Foundational, fixture) → 3. Phase 3 (US1 Payment) → **VALIDAR** `dotnet test --filter PaymentControllerTests` → demo MVP.

### Incremental Delivery

1. Setup + Foundational → fundação pronta.
2. US1 Payment → validar → entregar (MVP).
3. US2 Store → validar → entregar.
4. US3 Webhook → validar → entregar.
5. US4 GraphQL → validar → entregar.

### Parallel Team Strategy

Após o Phase 2: Dev A→US1, Dev B→US2, Dev C→US3, Dev D→US4; serializar apenas edições ao `TestDataHelper.cs`.

---

## Notes

- [P] = arquivos diferentes, sem dependências pendentes.
- Namespaces file-scoped (constituição §III) em todos os arquivos `.cs`.
- Nenhum segredo versionado — apenas placeholders `REPLACE_VIA_ENV_*` (constituição §V).
- Testes idempotentes, sem teardown (research.md R5); happy-path de `DELETE /store` não exercitado destrutivamente.
- Asserts com FluentAssertions + Flurl `.AllowAnyHttpStatus()`; URLs via `AppendPathSegment`/`SetQueryParam`.
- Não modificar código de produção; se um endpoint não for testável sem mudança, pausar e encaminhar a `dotnet-senior-developer`.
- Commit após cada tarefa ou grupo lógico.
