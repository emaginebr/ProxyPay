# Data Model: Testes de API para todos os endpoints existentes

**Feature**: 003-api-endpoint-tests | **Date**: 2026-06-26
**Phase**: 1 (Design)

Este projeto não introduz entidades de domínio nem esquema de banco. O "modelo de dados" aqui descreve (a) os artefatos de teste e (b) os DTOs de produção (de `ProxyPay.DTO`) que os testes exercitam como payloads/respostas.

## Artefatos de teste

### ApiTestFixture (`IAsyncLifetime`)
Contexto compartilhado por toda a sessão de testes (uma instância por execução, via `ICollectionFixture`).

| Campo/Membro | Tipo | Descrição |
|--------------|------|-----------|
| `BaseUrl` | string | `ApiBaseUrl` resolvida da config; raiz das requisições. |
| `AuthToken` | string | Token NAuth obtido no login único em `InitializeAsync`. |
| `Tenant` | string | Valor de `X-Tenant-Id` injetado em todas as requisições. |
| `CreateAuthenticatedRequest(path)` | `IFlurlRequest` | Anexa `Authorization: Basic {token}` + `X-Tenant-Id`. |
| `CreateAnonymousRequest(path)` | `IFlurlRequest` | Sem `Authorization`; mantém `X-Tenant-Id`. |
| `InitializeAsync()` | Task | Carrega config, valida placeholders, faz login NAuth. Falha rápido se algum `REPLACE_VIA_ENV_*` persistir. |

**Regra de validação**: qualquer valor de config iniciando com `REPLACE_VIA_ENV_` lança exceção nomeando a env var faltante antes de qualquer teste rodar.

### ApiTestCollection
`[CollectionDefinition("ApiTests")]` → `ICollectionFixture<ApiTestFixture>`. Todas as classes de teste declaram `[Collection("ApiTests")]` para compartilhar a fixture (login único).

### TestDataHelper
Factories de payload, uma `Create<DtoName>(...)` por DTO usado, com defaults sensatos. Sem factories órfãs.

### GraphQLQueries
Constantes string com as consultas GraphQL (`myStores`, `myInvoices`, `myInvoiceByNumber`, `myTransactions`, `myBalance`, `myCustomers`).

## DTOs de produção exercitados (de `ProxyPay.DTO`)

Os testes constroem e/ou desserializam estes DTOs existentes (não os modificam):

| DTO | Namespace | Usado em |
|-----|-----------|----------|
| `StoreInsertInfo` (name, email, billingStrategy) | `ProxyPay.DTO.Store` | `POST /store` |
| `StoreUpdateInfo` | `ProxyPay.DTO.Store` | `PUT /store` |
| `StoreApiKeyUpdateInfo` (apiKey) | `ProxyPay.DTO.Store` | `PUT /store/{id}/abacatepay-apikey` |
| `BillingRequest` (clientId, frequency, paymentMethod, customer, items, …) | `ProxyPay.DTO.Billing` | `POST /payment/billing` |
| `BillingResponse` | `ProxyPay.DTO.Billing` | resposta de `POST /payment/billing` |
| `InvoiceRequest` | `ProxyPay.DTO.Invoice` | `POST /payment/invoice` |
| `InvoiceResponse` | `ProxyPay.DTO.Invoice` | resposta de `POST /payment/invoice` |
| `QRCodeRequest` (clientId, customer, items) | `ProxyPay.DTO.Invoice` | `POST /payment/qrcode` |
| `QRCodeResponse` / `QRCodeStatusResponse` | `ProxyPay.DTO.Invoice` | `POST /payment/qrcode`, `GET /payment/qrcode/status/{id}` |
| `CustomerInsertInfo` | `ProxyPay.DTO.Customer` | sub-objeto `customer` dos payloads de pagamento |
| `AbacatePayWebhookPayload` (event, devMode, data) | `ProxyPay.DTO.AbacatePay` | `POST /webhook/abacatepay` |
| `GraphQLRequest` / `GraphQLResponse` | `ProxyPay.DTO.GraphQL` | `POST /graphql`, `POST /api/graphql-docs` |

## Recursos de teste (lógicos, sem persistência local)

| Recurso | Identidade | Ciclo de vida nos testes |
|---------|-----------|--------------------------|
| Store | `StoreId` (long), `ClientId` (string) | **Cada teste de pagamento cria a sua** via `POST /store` (helper `ApiTestFixture.CreateStoreAsync`), que ainda seta a AbacatePay API key na loja; o `ClientId` retornado alimenta o payload. **Não** deletada (idempotência R5). |
| Billing/Invoice/QRCode | `Id` / `invoiceId` (long) | Criados pelos próprios fluxos de teste; IDs reutilizados dentro do mesmo teste para status/simulação. |
| Webhook event | `event` (string), `data.id` (string), `secret` (query) | Simulado in-test; sem persistência além do efeito no estado da fatura no backend. |

## Configuração (appsettings.Test.json — placeholders)

| Chave | Env var | Sensível | Default sugerido |
|-------|---------|----------|------------------|
| `ApiBaseUrl` | `ApiBaseUrl` | Não | `http://localhost:5000` |
| `Auth:BaseUrl` | `Auth__BaseUrl` | Não | (NAuth API URL) |
| `Auth:Email` | `Auth__Email` | Sim | — |
| `Auth:Password` | `Auth__Password` | Sim | — |
| `Auth:Tenant` | `Auth__Tenant` | Não | (ex.: `emagine`) |
| `Auth:LoginEndpoint` | `Auth__LoginEndpoint` | Não | `/user/loginWithEmail` |
| `Auth:DeviceFingerprint` | `Auth__DeviceFingerprint` | Não | `proxypay-apitests` (NAuth vincula o token ao device) |
| `Store:AbacatePayApiKey` | `Store__AbacatePayApiKey` | Sim | — (API key da AbacatePay sandbox, setada na loja criada por teste) |
| `Webhook:Secret` | `Webhook__Secret` | Sim | — (segredo do webhook AbacatePay) |
| `Timeout` | `Timeout` | Não | `30` |
