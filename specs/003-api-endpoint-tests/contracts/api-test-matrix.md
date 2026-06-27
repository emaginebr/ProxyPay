# Contract: Matriz de testes por endpoint

**Feature**: 003-api-endpoint-tests | **Date**: 2026-06-26
**Phase**: 1 (Design)

Para um projeto de testes, o "contrato" é a superfície de API sob teste e o comportamento observável esperado de cada endpoint. Cada linha vira um ou mais métodos `<Method>_<Condition>_ShouldReturn<Expected>`. Status esperados derivam do código dos controllers e dos padrões de erro da constituição §6.

Convenção de asserts: FluentAssertions + Flurl `.AllowAnyHttpStatus()`.

---

## StoreController — `/store` (todos `[Authorize]`)

Arquivo: `Controllers/StoreControllerTests.cs`

| Endpoint | Cenário | Requisição | Esperado | Método de teste |
|----------|---------|-----------|----------|-----------------|
| `POST /store` | Sem auth | anônima | `401` | `Create_WithoutAuth_ShouldReturn401` |
| `POST /store` | Criação válida | auth + `StoreInsertInfo` válido | `201` + body com `storeId`, `clientId` | `Create_WithValidBody_ShouldReturn201` |
| `PUT /store` | Sem auth | anônima | `401` | `Update_WithoutAuth_ShouldReturn401` |
| `PUT /store` | Atualização válida | auth + `StoreUpdateInfo` da própria store | `204` | `Update_WithValidBody_ShouldReturn204` |
| `PUT /store` | Store de terceiro | auth + store de outro dono | `403` (Forbid) | `Update_ForNonOwnedStore_ShouldReturn403` |
| `PUT /store/{id}/abacatepay-apikey` | Sem auth | anônima | `401` | `SetAbacatePayApiKey_WithoutAuth_ShouldReturn401` |
| `PUT /store/{id}/abacatepay-apikey` | Válido (dono) | auth + `StoreApiKeyUpdateInfo` | `204` | `SetAbacatePayApiKey_WithValidBody_ShouldReturn204` |
| `PUT /store/{id}/abacatepay-apikey` | Store de terceiro | auth | `403` | `SetAbacatePayApiKey_ForNonOwnedStore_ShouldReturn403` |
| `DELETE /store/{id}` | Sem auth | anônima | `401` | `Delete_WithoutAuth_ShouldReturn401` |
| `DELETE /store/{id}` | Store de terceiro | auth | `403` | `Delete_ForNonOwnedStore_ShouldReturn403` |

> Nota R5 (idempotência): o happy-path de `DELETE` não é exercitado contra uma store persistente compartilhada para não remover dados; cobre-se o caminho de autorização (`401`/`403`). O happy-path de criação afirma contrato da resposta, não contagem global.

---

## PaymentController — `/payment` (anônimo)

Arquivo: `Controllers/PaymentControllerTests.cs`

| Endpoint | Cenário | Requisição | Esperado | Método de teste |
|----------|---------|-----------|----------|-----------------|
| `POST /payment/billing` | Válido | `BillingRequest` com customer+email + `clientId` de store de teste | `200` + `BillingResponse` | `CreateBilling_WithValidBody_ShouldReturnOk` |
| `POST /payment/billing` | Sem customer | `BillingRequest` com `customer = null` | `400` "Customer is required" | `CreateBilling_WithoutCustomer_ShouldReturn400` |
| `POST /payment/billing` | Customer sem email | customer sem `email` | `400` "Customer email is required" | `CreateBilling_WithoutCustomerEmail_ShouldReturn400` |
| `POST /payment/invoice` | Válido | `InvoiceRequest` válido | `200` + `InvoiceResponse` | `CreateInvoice_WithValidBody_ShouldReturnOk` |
| `POST /payment/invoice` | Sem customer | `customer = null` | `400` | `CreateInvoice_WithoutCustomer_ShouldReturn400` |
| `POST /payment/qrcode` | Válido | `QRCodeRequest` válido | `200` + `QRCodeResponse` | `CreateQRCode_WithValidBody_ShouldReturnOk` |
| `POST /payment/qrcode` | Customer sem email | customer sem `email` | `400` | `CreateQRCode_WithoutCustomerEmail_ShouldReturn400` |
| `GET /payment/qrcode/status/{invoiceId}` | Fatura existente | `invoiceId` criado no fluxo | `200` + `QRCodeStatusResponse` | `CheckQRCodeStatus_ForExistingInvoice_ShouldReturnOk` |
| `GET /payment/qrcode/status/{invoiceId}` | Fatura inexistente | id inválido | `400` (erro controlado) | `CheckQRCodeStatus_ForUnknownInvoice_ShouldReturn400` |
| `POST /payment/simulate-payment/{invoiceId}` | Fatura existente (sandbox) | `invoiceId` criado no fluxo | `200` + `AbacatePayResponse<PixQrCodeInfo>` | `SimulatePayment_ForExistingInvoice_ShouldReturnOk` |

---

## WebhookController — `/webhook` (anônimo, protegido por `secret`)

Arquivo: `Controllers/WebhookControllerTests.cs`

| Endpoint | Cenário | Requisição | Esperado | Método de teste |
|----------|---------|-----------|----------|-----------------|
| `POST /webhook/abacatepay` | Sem segredo | sem query `secret` | `200` (ignora silenciosamente — Padrão 3) | `Webhook_WithoutSecret_ShouldReturn200AndIgnore` |
| `POST /webhook/abacatepay` | Segredo inválido | `secret=errado` | `200` (ignora) | `Webhook_WithInvalidSecret_ShouldReturn200AndIgnore` |
| `POST /webhook/abacatepay` | Segredo válido, payload sem data | `secret` correto + payload sem `data` | `200` (não processa) | `Webhook_WithValidSecretMissingData_ShouldReturn200` |
| `POST /webhook/abacatepay` | Segredo válido, payload completo | `secret` correto + `AbacatePayWebhookPayload` completo | `200` (processa) | `Webhook_WithValidSecretAndPayload_ShouldReturn200` |

> O endpoint sempre retorna `200` por design (anti-retry, constituição §6 Padrão 3). Os testes verificam o status e — quando possível — efeito colateral observável via outro endpoint (ex.: status da fatura), sem depender de não determinismo do sandbox.

---

## GraphQL — `POST /graphql` (auth) + `POST /api/graphql-docs` (`[Authorize]`)

Arquivo: `Controllers/GraphQLControllerTests.cs`

| Endpoint | Cenário | Requisição | Esperado | Método de teste |
|----------|---------|-----------|----------|-----------------|
| `POST /graphql` | `myStores` autenticado | auth + `{ query: myStores }` | `200`, `data` presente, sem `errors` | `Graphql_MyStores_WithAuth_ShouldReturnData` |
| `POST /graphql` | Sem auth | anônima + `{ query: myStores }` | `401` ou `data=null`/erro de autorização | `Graphql_MyStores_WithoutAuth_ShouldBeUnauthorized` |
| `POST /graphql` | `myInvoices` autenticado | auth + query | `200` + `data` sem `errors` | `Graphql_MyInvoices_WithAuth_ShouldReturnData` |
| `POST /graphql` | `myInvoiceByNumber` autenticado | auth + query c/ arg | `200` sem `errors` de execução | `Graphql_MyInvoiceByNumber_WithAuth_ShouldReturnData` |
| `POST /graphql` | `myTransactions` autenticado | auth + query | `200` + `data` sem `errors` | `Graphql_MyTransactions_WithAuth_ShouldReturnData` |
| `POST /graphql` | `myBalance` autenticado | auth + query | `200` + `data` sem `errors` | `Graphql_MyBalance_WithAuth_ShouldReturnData` |
| `POST /graphql` | `myCustomers` autenticado | auth + query | `200` + `data` sem `errors` | `Graphql_MyCustomers_WithAuth_ShouldReturnData` |
| `POST /api/graphql-docs` | Sem auth | anônima | `401` | `GraphqlDocs_WithoutAuth_ShouldReturn401` |
| `POST /api/graphql-docs` | Autenticado | auth + `GraphQLRequest` | `200` (stub) | `GraphqlDocs_WithAuth_ShouldReturn200` |

---

## Cobertura ↔ requisitos

| Requisito | Coberto por |
|-----------|-------------|
| FR-001 (todos os endpoints) | Todas as tabelas acima (4 controllers + GraphQL) |
| FR-005 (sucesso por endpoint) | Linhas "Válido"/"autenticado" de cada tabela |
| FR-006 (falha de validação) | Linhas `400` de Payment |
| FR-007 (rejeição anônima) | Linhas `401` de Store e GraphQL docs/graphql |
| FR-008 (propriedade de recurso) | Linhas `403` de Store |
| FR-009 (webhook segredo) | Tabela Webhook |
| FR-010 (GraphQL autenticado) | Tabela GraphQL |
| FR-013/R5 (idempotência) | Nota de idempotência em Store e Webhook |
| FR-014 (sandbox) | `SimulatePayment...`, Payment, Webhook |
