# Implementation Plan: Testes de API para todos os endpoints existentes

**Branch**: `003-api-endpoint-tests` | **Date**: 2026-06-26 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-api-endpoint-tests/spec.md`

## Summary

Criar um projeto de testes de API dedicado — `ProxyPay.ApiTests` — que exercita, via HTTP externo, **todos os endpoints atualmente expostos** pela API ProxyPay (Store, Payment, Webhook, GraphQL e o stub GraphQL docs). A suíte usa o stack da skill `dotnet-test-api` (xUnit + Flurl.Http + FluentAssertions) com uma fixture compartilhada `IAsyncLifetime` que autentica **uma única vez por sessão** e injeta os cabeçalhos exigidos pela arquitetura multi-tenant em cada requisição. Os testes são idempotentes (sem etapa de limpeza), assumem a API já em execução numa `ApiBaseUrl` configurável e exercitam a AbacatePay em modo sandbox/DevMode. Cada endpoint recebe ao menos um cenário de sucesso; endpoints `[Authorize]` recebem também um cenário de rejeição anônima; endpoints com validação recebem um cenário de falha de validação.

## Technical Context

**Language/Version**: C# / .NET 8.0 (alinhado à solution `backend/ProxyPay.sln`)
**Primary Dependencies**: xunit 2.5, xunit.runner.visualstudio 2.5, Microsoft.NET.Test.Sdk 17.8, FluentAssertions 7.0, Flurl.Http 4.0, Microsoft.Extensions.Configuration(.Json/.EnvironmentVariables) 9.0, coverlet.collector 6.0
**Storage**: N/A (a suíte não acessa banco diretamente; toda interação é via HTTP contra a API em execução)
**Testing**: xUnit (test framework); execução via `dotnet test`
**Target Platform**: API .NET 8 em execução, alcançável por `ApiBaseUrl` (instância local/dev)
**Project Type**: Projeto de testes de integração externa (`backend/ProxyPay.ApiTests`), separado de `ProxyPay.Tests` (testes unitários)
**Performance Goals**: N/A (suíte de testes; o objetivo é cobertura e determinância, não throughput)
**Constraints**: Comunicação exclusivamente via HTTP externo (a suíte não sobe a API in-process); nenhum segredo real versionado (placeholders `REPLACE_VIA_ENV_*` + variáveis de ambiente); testes idempotentes sem limpeza; AbacatePay em sandbox/DevMode
**Scale/Scope**: 4 controllers + 1 endpoint GraphQL → 5 classes de teste; ~11 endpoints HTTP cobertos (4 Store, 5 Payment, 1 Webhook, 1 GraphQL docs) + consultas GraphQL autenticadas

### Esquema de autenticação (decisão de adaptação)

A API usa **NAuth Basic Authentication** (`services.AddNAuthAuthentication("BasicAuthentication")`, ver `ProxyPay.Application/Startup.cs:96`) — header `Authorization: Basic {token}` — combinado com o header de tenant `X-Tenant-Id` (ver `ProxyPay.API/Middlewares/TenantMiddleware.cs`). O preset "Generic JWT Bearer" padrão da skill **não se aplica**; a fixture adota o preset **NAuth** adaptado: faz login uma vez no endpoint de autenticação NAuth (configurável) e anexa `Authorization: Basic {token}` + `X-Tenant-Id` em **todas** as requisições autenticadas. Detalhes do fluxo resolvidos em `research.md`.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Avaliação contra `.specify/memory/constitution.md` v1.1.0:

| Princípio | Aplicabilidade ao projeto de testes | Status |
|-----------|-------------------------------------|--------|
| I. Skills Obrigatórias (`dotnet-architecture`) | Aplica-se a entidades/services/repos **de produção**. Este trabalho é exclusivamente código de teste, governado pela skill `dotnet-test-api`. Nenhuma entidade/DTO/migration de produção é criada. | ✅ PASS |
| II. Stack Tecnológica Bloqueada | A stack de produção (EF Core, PostgreSQL, NAuth) não é estendida. Pacotes adicionados são **test-only** (xUnit/Flurl/FluentAssertions), fora da tabela bloqueada. Nenhum comando `docker`/`docker compose` é executado. | ✅ PASS |
| III. Convenções de Código (.NET) | Código C# segue PascalCase, campos `_camelCase`. **Adaptação**: os templates da skill usam namespaces com bloco `{ }`; a constituição exige `namespace X;` file-scoped — os arquivos gerados usarão a forma file-scoped para conformidade. DTOs não são criados aqui. | ✅ PASS (com adaptação documentada) |
| IV. Convenções de Banco (PostgreSQL) | Nenhuma alteração de esquema, migration ou DbContext. | ✅ N/A |
| V. Autenticação e Segurança | Nenhum secret versionado: `appsettings.Test.json` usa apenas placeholders `REPLACE_VIA_ENV_*`, resolvidos por variáveis de ambiente (alinhado às Restrições Adicionais > Variáveis de Ambiente). A fixture falha rápido se um placeholder não for resolvido. Tokens/segredos nunca são impressos em asserts. | ✅ PASS |
| 6. Padrão de Tratamento de Erros | A suíte **valida** os padrões observáveis (400 validação = Padrão 1; webhook `Ok()` mesmo com segredo inválido = Padrão 3). Não altera controllers. | ✅ PASS |

**Resultado do gate**: PASS. Nenhuma violação que exija Complexity Tracking. A única nota (namespaces file-scoped) é uma adaptação de conformidade, não uma exceção.

## Project Structure

### Documentation (this feature)

```text
specs/003-api-endpoint-tests/
├── plan.md              # Este arquivo (/speckit.plan)
├── research.md          # Phase 0 — decisões (auth NAuth Basic, sandbox, BaseUrl)
├── data-model.md        # Phase 1 — entidades de teste e DTOs exercitados
├── quickstart.md        # Phase 1 — como configurar e rodar a suíte
├── contracts/           # Phase 1 — matriz de contrato de teste por endpoint
│   └── api-test-matrix.md
├── checklists/
│   └── requirements.md  # Checklist de qualidade da spec (já existente)
└── tasks.md             # Phase 2 (/speckit.tasks — NÃO criado por /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── ProxyPay.sln                      # add ProxyPay.ApiTests
├── ProxyPay.Tests/                   # testes unitários (existente — não tocar)
└── ProxyPay.ApiTests/                # NOVO — este projeto
    ├── ProxyPay.ApiTests.csproj      # refs: ProxyPay.DTO (única referência)
    ├── appsettings.Test.json         # placeholders REPLACE_VIA_ENV_*
    ├── Fixtures/
    │   ├── ApiTestFixture.cs         # IAsyncLifetime — login NAuth Basic + X-Tenant-Id
    │   └── ApiTestCollection.cs      # [CollectionDefinition("ApiTests")]
    ├── Controllers/
    │   ├── StoreControllerTests.cs       # create/update/set-apikey/delete + 401/403
    │   ├── PaymentControllerTests.cs     # billing/invoice/qrcode/status/simulate + validação
    │   ├── WebhookControllerTests.cs     # segredo inválido/ausente/válido
    │   └── GraphQLControllerTests.cs     # /graphql consultas autenticadas + /api/graphql-docs
    └── Helpers/
        ├── TestDataHelper.cs         # factories de payload por DTO
        └── GraphQLQueries.cs         # strings das queries myStores/myInvoices/...
```

**Structure Decision**: Projeto único `backend/ProxyPay.ApiTests`, no nível da solution, irmão de `ProxyPay.Tests` (conforme a convenção da skill `dotnet-test-api`). Uma classe de teste por controller; uma classe extra para as consultas GraphQL (que não passam por um controller MVC, mas pelo endpoint `/graphql` mapeado em `Startup.cs:109`). A referência de projeto é exclusivamente `ProxyPay.DTO` — nunca Domain/Application/Infra/API.

## Complexity Tracking

> Nenhuma violação constitucional a justificar. Tabela não aplicável.
