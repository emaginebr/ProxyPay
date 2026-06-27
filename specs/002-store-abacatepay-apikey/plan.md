# Implementation Plan: API_KEY do AbacatePay por Loja

**Branch**: `002-store-abacatepay-apikey` | **Date**: 2026-06-26 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-store-abacatepay-apikey/spec.md`

## Summary

Cada loja (`Store`) passa a ter sua própria credencial do AbacatePay, armazenada na coluna já existente `proxypay_stores.abacatepay_api_key`. A credencial é **write-only**: alterável apenas por um endpoint dedicado (`PUT /store/{storeId}/abacatepay-apikey`), nunca retornada por REST nem GraphQL (somente um indicador booleano `hasAbacatePayApiKey`). As operações de pagamento (Invoice/Billing/QR Code) passam a usar a credencial da loja envolvida; se a loja não tiver credencial própria, a operação é recusada com erro claro — **sem** fallback para a chave global do ambiente. A coleção Bruno ganha a requisição correspondente.

Abordagem técnica central: a coluna física e o `ModelSnapshot` já contêm `AbacatePayApiKey`; portanto **não é necessária nova migration** — basta mapear a propriedade. A propagação da chave por loja é feita estendendo `IAbacatePayAppService` para receber a `apiKey` por chamada, com os domain services resolvendo a chave da loja antes de invocar o provedor.

## Technical Context

**Language/Version**: C# / .NET 8.0
**Primary Dependencies**: EF Core 9 (Npgsql), HotChocolate (GraphQL), AutoMapper, FluentValidation, NAuth (Basic token), Newtonsoft.Json (cliente AbacatePay)
**Storage**: PostgreSQL — um banco por tenant; coluna `proxypay_stores.abacatepay_api_key varchar(500) NULL` **já existente**
**Testing**: xUnit (skill `dotnet-test`) — projeto único `.Tests` espelhando as camadas
**Target Platform**: Linux server (API ASP.NET Core), multi-tenant por connection string
**Project Type**: Web service (backend .NET) — alterações restritas ao `backend/` + coleção Bruno
**Performance Goals**: N/A (sem mudança de carga; uma leitura adicional de Store por operação de pagamento)
**Constraints**: Credencial nunca legível pela API (write-only); sem fallback para chave de ambiente; sem criptografia adicional em repouso (FR-015); isolamento por tenant preservado
**Scale/Scope**: 3 tenants atuais (emagine, monexup, fortuno), 1 loja por usuário; mudança localizada em ~10 arquivos backend + 1 request Bruno

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Avaliação | Status |
| --------- | --------- | ------ |
| I. Skills Obrigatórias | Entidade/DTO/Service/DI/mapeamento serão criados/alterados via skill `dotnet-architecture`; validação via `dotnet-fluent-validation`; testes via `dotnet-test` | ✅ PASS |
| II. Stack Bloqueada | Nenhuma tecnologia nova; EF Core continua único ORM; nenhum comando `docker` local | ✅ PASS |
| III. Convenções de Código | DTO novo com `[JsonPropertyName(camelCase)]`; namespaces file-scoped; `_camelCase` em campos privados | ✅ PASS |
| IV. Convenções de Banco | Coluna `abacatepay_api_key` já em `snake_case`, `varchar(500)`, nullable; sem cascade | ✅ PASS |
| V. Autenticação e Segurança | Endpoint sob `[Authorize]` + validação de ownership; credencial **nunca** retornada (write-only) — alinhado à regra inegociável de não vazar secrets em respostas/logs/erros | ✅ PASS |
| Tratamento de Erros | Padrão 1 (`BadRequest`) para validação/ownership/loja inexistente; Padrão 2 (`StatusCode(500)`) como fallback; sem stack trace nem secret em mensagens | ✅ PASS |

**Resultado**: PASS — sem violações. Complexity Tracking não aplicável.

> Nota de segurança adicional: a credencial é um secret. Garantir que ela **não apareça em logs** — diferente do payload do AbacatePay que hoje é logado em `AbacatePayAppService`, a `apiKey` só entra no header `Authorization` e **não deve** ser incluída em nenhum `LogInformation`.

## Project Structure

### Documentation (this feature)

```text
specs/002-store-abacatepay-apikey/
├── plan.md              # Este arquivo (/speckit.plan)
├── research.md          # Phase 0 — decisões técnicas (migration, fallback, GraphQL hiding)
├── data-model.md        # Phase 1 — entidade Store + atributo da credencial
├── quickstart.md        # Phase 1 — como exercitar (Bruno + verificação write-only)
├── contracts/
│   ├── set-abacatepay-apikey.http.md   # Contrato REST do endpoint dedicado
│   └── store-graphql.contract.md       # Contrato GraphQL (campo oculto + indicador)
├── checklists/
│   └── requirements.md  # Já existente (/speckit.specify)
└── tasks.md             # Phase 2 (/speckit.tasks — NÃO criado aqui)
```

### Source Code (repository root)

```text
backend/
├── ProxyPay.DTO/
│   └── Store/
│       └── StoreApiKeyUpdateInfo.cs            # NOVO — DTO { apiKey }
├── ProxyPay.Domain/
│   ├── Models/
│   │   └── StoreModel.cs                       # + AbacatePayApiKey + SetAbacatePayApiKey()
│   ├── Interfaces/
│   │   └── IStoreService.cs                    # + UpdateAbacatePayApiKeyAsync(...)
│   └── Services/
│       ├── StoreService.cs                     # + implementação UpdateAbacatePayApiKeyAsync
│       ├── InvoiceService.cs                   # resolve chave da loja; passa para AbacatePay
│       └── BillingService.cs                   # resolve chave da loja; passa para AbacatePay
├── ProxyPay.Infra.Interfaces/
│   └── AppServices/
│       └── IAbacatePayAppService.cs            # métodos passam a receber string apiKey
├── ProxyPay.Infra/
│   ├── Context/
│   │   ├── Store.cs                            # + propriedade AbacatePayApiKey
│   │   └── ProxyPayContext.cs                  # + Fluent mapping da coluna
│   ├── Mappers/
│   │   └── StoreProfile.cs                     # Ignore AbacatePayApiKey em Insert/Update DTO→Model
│   └── AppServices/
│       └── AbacatePayAppService.cs             # CreateClient(apiKey); sem chave global obrigatória
├── ProxyPay.GraphQL/
│   └── Types/
│       └── StoreType.cs                        # NOVO — oculta AbacatePayApiKey, expõe hasAbacatePayApiKey
└── ProxyPay.API/
    └── Controllers/
        └── StoreController.cs                  # + endpoint PUT /store/{storeId}/abacatepay-apikey

bruno/
└── Store/
    └── Set ApiKey.bru                          # NOVO — request do endpoint dedicado
```

**Structure Decision**: Web service em Clean Architecture já existente sob `backend/`. A feature segue o fluxo DTO → Domain (Model/Interface/Service) → Infra (Context/Mapper/AppService) → API (Controller), mais a camada GraphQL para o field hiding. Sem alterações no `frontend/` (fora do escopo do pedido).

## Phase 0 — Research

Ver [research.md](./research.md). Decisões-chave resolvidas:

1. **Sem nova migration** — coluna e snapshot já contêm `AbacatePayApiKey`; gerar migration agora exporia um drift pré-existente no snapshot (faltam `client_id`/`billing_strategy`) e produziria `AddColumn` indevido. Apenas mapear a propriedade.
2. **Propagação da chave por chamada** — `IAbacatePayAppService` recebe `apiKey` por método; domain services resolvem a chave da loja via `IStoreRepository`.
3. **Sem fallback** — chave ausente/vazia ⇒ `BadRequest`/exceção clara antes de chamar o provedor (FR-014).
4. **Field hiding GraphQL** — `StoreType : ObjectType<Store>` com `Ignore()` no campo + resolver booleano `hasAbacatePayApiKey` consultando o contexto (evita armadilha de projeção).
5. **AutoMapper protege a chave** — `Ignore()` nos mapas `StoreInsertInfo→StoreModel` e `StoreUpdateInfo→StoreModel` para o update geral não sobrescrever a credencial com null.

## Phase 1 — Design & Contracts

- [data-model.md](./data-model.md) — atributo `AbacatePayApiKey` na entidade/model, regras de validação e estados.
- [contracts/set-abacatepay-apikey.http.md](./contracts/set-abacatepay-apikey.http.md) — contrato do endpoint REST dedicado.
- [contracts/store-graphql.contract.md](./contracts/store-graphql.contract.md) — contrato do tipo `Store` no GraphQL (campo oculto + indicador booleano).
- [quickstart.md](./quickstart.md) — passos de verificação manual (Bruno + checagem write-only em REST e GraphQL).

## Complexity Tracking

> Sem violações de constituição. Seção não aplicável.
