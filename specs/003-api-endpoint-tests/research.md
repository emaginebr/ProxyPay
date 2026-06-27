# Research: Testes de API para todos os endpoints existentes

**Feature**: 003-api-endpoint-tests | **Date**: 2026-06-26
**Phase**: 0 (Outline & Research)

Resolve os pontos técnicos do plano antes do design. Cada item segue o formato Decisão / Justificativa / Alternativas consideradas.

---

## R1 — Esquema de autenticação da fixture

**Decisão**: A fixture `ApiTestFixture` (`IAsyncLifetime`) autentica **uma vez por sessão** contra a NAuth API (URL configurável `Auth:BaseUrl`) e, em cada requisição autenticada, anexa `Authorization: Basic {token}` **e** o header `X-Tenant-Id: {tenant}`. Adota o preset **NAuth** da skill `dotnet-test-api`, adaptado de `WithOAuthBearerToken` (Bearer) para `WithHeader("Authorization", $"Basic {token}")`.

**Justificativa**: A API registra `services.AddNAuthAuthentication("BasicAuthentication")` (`ProxyPay.Application/Startup.cs:96`) e a constituição §V fixa o esquema como `Authorization: Basic {token}`. O `TenantMiddleware` (`ProxyPay.API/Middlewares/TenantMiddleware.cs`) lê `X-Tenant-Id` para resolver o tenant; sem ele as queries multi-tenant não resolvem o banco correto. O frontend (`nauth-react`, `VITE_NAUTH_API_URL`) já autentica desta forma, confirmando que existe um endpoint de login NAuth real.

**Alternativas consideradas**:
- *Bearer JWT (default da skill)*: rejeitado — a API não usa Bearer; o handler é Basic.
- *Auto-emissão do Basic token usando o `JwtSecret` do tenant* (como sugerido no cabeçalho de `ProxyPay.Tests/Integration/CrossTenantIsolationTests.cs`): rejeitado para a suíte externa — exigiria o segredo do tenant embutido na fixture e acoplaria o teste à criptografia interna do NAuth, contrariando o teste black-box via HTTP. Mantido apenas como fallback documentado se a NAuth API de login não estiver disponível no ambiente.

---

## R2 — Endpoint de login NAuth

**Decisão**: O caminho de login é configurável via `Auth:LoginEndpoint`, com default `/user/loginWithEmail` (convenção NAuth usada pelo preset da skill). A fixture faz `POST { email, password }` e extrai o token da resposta. O valor exato é confirmado contra a instância NAuth em execução durante a implementação; se divergir, ajusta-se apenas a configuração, sem mudança de código.

**Justificativa**: O endpoint concreto está encapsulado em `nauth-react` e na NAuth API externa; torná-lo configuração (não constante) mantém a fixture estável entre ambientes (local/dev/staging) e evita hard-coding. A política de segredos da skill já prevê `Auth:BaseUrl` configurável.

**Alternativas consideradas**:
- *Hard-code do path*: rejeitado — reduz portabilidade entre ambientes.
- *Descobrir via Swagger da NAuth*: desnecessário — o default cobre a convenção e é sobreponível por env var.

---

## R3 — Dependência externa AbacatePay (sandbox/DevMode)

**Decisão**: A API sob teste é configurada (via seu próprio ambiente) contra o **sandbox/DevMode** da AbacatePay. Os testes de Payment e Webhook fazem **assertions de contrato** (status HTTP, forma da resposta, presença de campos), tolerando variação de valores não determinísticos. O endpoint `POST /payment/simulate-payment/{invoiceId}` e o campo `DevMode` do webhook (`AbacatePayWebhookPayload`) confirmam a existência do modo sandbox.

**Justificativa**: Clarificação Q1 da spec. Sandbox dá respostas reais e estáveis o suficiente para validar contrato sem criar transações de produção, mantendo a suíte como teste de integração externa genuíno.

**Alternativas consideradas**:
- *Stub/fake local*: rejeitado pela clarificação — exigiria infra de stub e configuração do ambiente da API para apontar ao fake.
- *AbacatePay real de produção*: rejeitado — efeitos colaterais e flakiness.

---

## R4 — Provisionamento da API sob teste

**Decisão**: A suíte **assume a API já em execução** numa `ApiBaseUrl` configurável e comunica-se via HTTP externo. A suíte **não** sobe a API (sem `WebApplicationFactory`, sem container). Orquestração de subida da API fica fora do escopo (documentada no `quickstart.md`).

**Justificativa**: Clarificação Q3 da spec e fronteira explícita da skill `dotnet-test-api` ("the skill assumes the API is reachable at `ApiBaseUrl`"). Mantém o teste mais próximo do comportamento de produção e desacopla o ciclo de testes do bootstrap da aplicação.

**Alternativas consideradas**:
- *In-process (`WebApplicationFactory<Startup>`)*: rejeitado pela clarificação — não é HTTP totalmente externo e não exercita o ambiente real (tenant middleware, NAuth real).
- *Container/processo externo orquestrado pela suíte*: rejeitado — complexidade de orquestração; constituição §II proíbe `docker` local.

---

## R5 — Idempotência e ausência de limpeza

**Decisão**: Os testes são **idempotentes** e **não** executam limpeza. Estratégias para tolerar dados residuais:
- Testes de criação (ex.: `POST /store`) afirmam o **contrato** da resposta (status `201`, presença de `storeId`/`clientId`), não a contagem total de registros.
- Testes que precisam de um recurso existente (status de QR code, simulate-payment) usam IDs criados no próprio fluxo do teste ou IDs de configuração conhecidos, e afirmam contrato, não estado global.
- Nenhum teste depende de um banco/tenant "limpo" no início.

**Justificativa**: Clarificação Q2 da spec. Evita acoplamento entre execuções e dispensa endpoints de teardown, ao custo de asserts focados em contrato em vez de estado absoluto.

**Alternativas consideradas**:
- *Auto-limpeza por teste* (criar e deletar): rejeitado pela clarificação; além disso o `DELETE /store/{id}` real teria efeitos sobre dados de pagamento relacionados.
- *Tenant/banco descartável por execução*: rejeitado pela clarificação — entra em orquestração de ambiente (fora do escopo, ver R4).

---

## R6 — Cobertura do endpoint GraphQL

**Decisão**: As consultas GraphQL autenticadas (`myStores`, `myInvoices`, `myInvoiceByNumber`, `myTransactions`, `myBalance`, `myCustomers`) são exercitadas via `POST /graphql` com corpo `{ "query": "..." }`. A suíte valida o **contrato** de cada consulta: autenticada → resposta `200` com `data` e sem `errors`; anônima → `data` ausente/erro de autorização. Não valida cada campo de cada tipo. O stub `POST /api/graphql-docs` (`[Authorize]`) recebe um teste de 401 anônimo e de 200 autenticado.

**Justificativa**: Clarificação/assumption da spec sobre profundidade de GraphQL. O endpoint único `/graphql` é mapeado em `ProxyPay.API/Startup.cs:109`; as consultas vivem em `ProxyPay.GraphQL/Admin/AdminQuery.cs`.

**Alternativas consideradas**:
- *Validar todos os campos de todos os tipos*: rejeitado — explosão de casos sem ganho de detecção de regressão de contrato.

---

## Pacotes e versões (consolidado)

| Pacote | Versão mínima | Finalidade |
|--------|---------------|------------|
| xunit | 2.5.3 | Framework de teste |
| xunit.runner.visualstudio | 2.5.3 | Runner |
| Microsoft.NET.Test.Sdk | 17.8.0 | Entrypoint `dotnet test` |
| FluentAssertions | 7.0.0 | Asserts `.Should()` |
| Flurl.Http | 4.0.2 | Cliente HTTP fluente |
| Microsoft.Extensions.Configuration(.Json/.EnvironmentVariables) | 9.0.8 | Config + env override |
| coverlet.collector | 6.0.0 | Cobertura |

Referência de projeto: somente `ProxyPay.DTO` (candidato único detectado pela varredura de sufixos `.DTO`).

---

## Itens NEEDS CLARIFICATION remanescentes

Nenhum. Todas as incógnitas do Technical Context foram resolvidas (R1–R6). O caminho de login NAuth (R2) é uma configuração sobreponível com default seguro, não um bloqueio de design.
