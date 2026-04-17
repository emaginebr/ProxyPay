# Implementation Plan: Multi-Tenant com tenant inicial "fortuno"

**Branch**: `001-multi-tenant-fortuno` | **Date**: 2026-04-17 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-multi-tenant-fortuno/spec.md`

## Summary

Esta feature completa a multi-tenancy do ProxyPay. Durante a investigação foi
confirmado que a **infraestrutura base já existe**: `TenantMiddleware`,
`TenantContext`, `TenantResolver`, `TenantDbContextFactory` e
`NAuthTenantSecretProvider` já estão presentes, DI configurada em
`ProxyPay.Application/Startup.cs`, e o catálogo de tenants reside em
`appsettings.*.json` sob a chave `Tenants:{tenantId}`. O que falta entregar é
(1) fechar os gaps de isolamento (middleware que silenciosamente usa
`DefaultTenantId` quando o cabeçalho falta; endpoints públicos sem
enforcement; bancos compartilhados em Dev), (2) remover o tenant legado
`monexup` e introduzir **`fortuno`** como segundo tenant, (3) tratar os
webhooks externos da AbacatePay (que não sabem sobre tenants) via rota
específica por tenant, (4) montar propagação automática do tenant em chamadas
ACL outbound, (5) criar o runner de migrações cross-tenant para `dotnet ef`
atuar em todos os bancos, e (6) executar a migração única da base de
produção atual para o banco dedicado do tenant **`emagine`**, com o banco
dedicado de **`fortuno`** inicializado vazio. Implementação será guiada pela
skill `dotnet-multi-tenant`, replicando padrões já validados em
`c:\repos\Lofn\Lofn`.

## Technical Context

**Language/Version**: C# / .NET 8.0
**Primary Dependencies**: EF Core 9.x (Npgsql), NAuth (Basic auth + per-tenant JWT via `ITenantSecretProvider`), zTools (S3/e-mail/slugs), HotChocolate GraphQL (admin schema), AutoMapper, FluentValidation, Swashbuckle 8.x
**Storage**: PostgreSQL — um banco dedicado por tenant (lazy-loading proxies habilitados); `ProxyPayContext` único instanciado via `TenantDbContextFactory` com connection string resolvida por requisição
**Testing**: xUnit (projeto `ProxyPay.Tests` já existe); testes de integração rodam contra instâncias PostgreSQL locais sem Docker (conforme constituição)
**Target Platform**: Linux server (produção) / Windows dev host; API hospedada como ASP.NET Core; execução local via `dotnet run` direto nos projetos, sem `docker compose`
**Project Type**: Web-service (.NET Clean Architecture backend) — sem alterações no layout de projetos nesta feature
**Performance Goals**: Sem SLA numérico nesta entrega (não priorizado pelo usuário em `/speckit.clarify`); objetivo é preservar a performance atual, sem regressão mensurável atribuível à multi-tenancy
**Constraints**: Cabeçalho HTTP de tenant obrigatório em **toda** requisição (inclusive anônimas — GraphQL, webhooks, payment); tokens de um tenant não podem ser aceitos em outro; segredos jamais em respostas/logs; sem Docker em dev local; migrações EF Core aplicáveis a todos os bancos de forma auditável
**Scale/Scope**: 2 tenants ativos na estreia (`emagine`, `fortuno`); arquitetura pronta para um terceiro tenant sem alteração de código; volume atual de dados do ProxyPay é absorvido integralmente por `emagine` via migração única; `fortuno` nasce vazio

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Avaliação contra os cinco princípios inegociáveis de
`.specify/memory/constitution.md` **v1.1.0** (amendada em 2026-04-17):

| # | Princípio                                   | Status | Observação |
|---|---------------------------------------------|--------|------------|
| I | Skills Obrigatórias (`dotnet-architecture`) | PASS   | Toda nova entidade backend (ex.: `TenantCatalogModel` se existir, serviços de migração cross-tenant) seguirá `/dotnet-architecture`. Camadas de DTO/Domain/Infra.Interfaces/Infra/Application respeitadas. |
| II | Stack Tecnológica Bloqueada               | PASS   | .NET 8, EF Core 9 Npgsql, PostgreSQL, NAuth, Swashbuckle — nenhum ORM alternativo introduzido. Nenhum comando `docker`/`docker compose` exigido em Dev. |
| III | Convenções de Código (.NET)              | PASS   | PascalCase/file-scoped namespaces/`[JsonPropertyName("camelCase")]` mantidos. Novos tipos seguirão o padrão existente em `ProxyPay.Application`. |
| IV | Convenções de Banco (PostgreSQL)          | PASS   | Snake_case plural, `{entidade}_id` bigint, `ClientSetNull`, `timestamp without time zone`, `varchar` com `MaxLength`. **Sem colunas `tenant_id` nas tabelas** — isolamento é físico por banco (conforme assumption da spec). Migrações novas não adicionam colunas; apenas são replicadas por banco via runner cross-tenant. |
| V | Autenticação e Segurança                   | PASS   | Basic via NAuth mantida; `[Authorize]` permanece nos controllers sensíveis; segredos por tenant continuam em `Tenants:{id}:JwtSecret` (nunca expostos). CORS `AllowAnyOrigin` no `Startup.cs` está coberto pela **exceção transitória** de §V da constituição v1.1.0 (válida até 2026-07-31), portanto não constitui violação enquanto a exceção estiver vigente; a feature **não afasta nem aproxima** o repositório da conformidade final (não toca CORS). |
| §6 | Padrão de Tratamento de Erros (Restrições Adicionais) | PASS | A constituição v1.1.0 reconhece três respostas-padrão aceitas: `BadRequest(message)` (padrão 1 — falhas de validação/cliente), `StatusCode(500, ex.Message)` (padrão 2 — fallback genérico) e `Ok()` + log interno (padrão 3 — webhooks anti-retry). `PaymentController` usa o padrão 1 (falhas de validação do `ClientId`/`Customer`) e `WebhookController` usa o padrão 3 (AbacatePay) — ambos legítimos. Nenhuma alteração de tratamento é necessária. |

**Observações adicionais**:

- O CLAUDE.md da raiz descreve um domínio e-commerce (produtos, pedidos,
  categorias) que **não corresponde** ao código real (stores, customers,
  invoices, billings, transactions — processamento de pagamento). Esta spec
  e plano tratam do domínio real. Atualização do CLAUDE.md continua como
  task de Polish (T041 em `tasks.md`).

**Veredicto inicial** (pré-Phase 0): Nenhuma violação. Avançar para Phase 0.

**Re-avaliação pós-design** (após `research.md`, `data-model.md`, contratos
e `quickstart.md`):

- Os novos artefatos runtime introduzidos — `ITenantCatalog`/`TenantCatalog`,
  `TenantHeaderHandler` (DelegatingHandler), `TenantResolverScope`,
  `MigrationRunner` — residem em `ProxyPay.Application` e `ProxyPay.API`
  seguindo os limites de camada da skill `dotnet-architecture`.
- A migração operacional (R-008) não altera schema nem nomes de tabela —
  apenas nomeia novos bancos físicos (`proxypay_emagine`, `proxypay_fortuno`)
  em conformidade com o Princípio IV.
- O contrato de rotas (`contracts/tenant-header.md`,
  `contracts/webhook-tenant-routing.md`) reforça FR-004, FR-012, FR-013 sem
  introduzir endpoints que contradigam a seção 4 da constituição (todos os
  novos fluxos preservam `[Authorize]` onde hoje existe e respeitam
  o padrão Basic/NAuth).
- CORS `AllowAnyOrigin`: antes da emenda v1.1.0, R-010 marcava como
  follow-up fora de escopo. Com a exceção transitória explícita de §V
  (válida até 2026-07-31), R-010 foi atualizado para refletir o novo
  status — não é mais um "pending follow-up ambíguo", é uma dívida datada
  que vence em outra feature.
- Tratamento de erros: `PaymentController` (`BadRequest`) e
  `WebhookController` (`Ok()`) encontram agora cobertura explícita nos
  padrões 1 e 3 da constituição v1.1.0 — o plano não precisa introduzir
  remediação.

**Veredicto pós-design**: Nenhuma nova violação. Pronto para `/speckit.tasks`.

## Project Structure

### Documentation (this feature)

```text
specs/001-multi-tenant-fortuno/
├── plan.md              # Este arquivo
├── research.md          # Phase 0 — decisões técnicas investigadas
├── data-model.md        # Phase 1 — modelagem de dados e contratos de tenant
├── quickstart.md        # Phase 1 — roteiro de bring-up local + produção
├── contracts/           # Phase 1 — contrato de cabeçalho de tenant + erros
│   ├── tenant-header.md
│   └── webhook-tenant-routing.md
├── checklists/
│   └── requirements.md  # Checklist de qualidade da spec (já completo)
└── tasks.md             # Saída do /speckit.tasks (ainda não gerado)
```

### Source Code (repository root — layout atual do ProxyPay)

```text
C:\repos\ProxyPay\ProxyPay\
├── ProxyPay.sln
├── ProxyPay.API/                     # REST + middleware + Startup HTTP
│   ├── Controllers/
│   │   ├── StoreController.cs        # [Authorize] — já OK via middleware
│   │   ├── PaymentController.cs      # GAP: público, sem enforcement de tenant
│   │   ├── WebhookController.cs      # GAP: recebe de AbacatePay sem tenant
│   │   └── GraphQLController.cs
│   ├── Middlewares/
│   │   └── TenantMiddleware.cs       # GAP: não rejeita quando header ausente
│   ├── Filters/                      # GlobalExceptionFilter
│   ├── Startup.cs                    # Pipeline já tem UseMiddleware<TenantMiddleware>
│   └── appsettings.{,Development,Docker,Production}.json  # ALTERAR: monexup → fortuno
├── ProxyPay.Application/             # DI + tenant infra
│   ├── Startup.cs                    # ConfigureProxyPay — DI do tenant já presente
│   ├── TenantContext.cs              # OK
│   ├── TenantResolver.cs             # GAP: fallback silencioso para DefaultTenantId
│   ├── TenantDbContextFactory.cs     # OK
│   ├── NAuthTenantSecretProvider.cs  # OK (per-tenant JWT secret)
│   └── NAuthTenantProvider.cs        # OK (liga NAuth ao TenantContext)
├── ProxyPay.Domain/
│   ├── Interfaces/
│   │   ├── ITenantContext.cs         # OK
│   │   └── ITenantResolver.cs        # OK
│   ├── Models/                       # StoreModel, CustomerModel, InvoiceModel, etc.
│   └── Services/                     # InvoiceService, TransactionService, etc.
├── ProxyPay.DTO/                     # Sem alteração estrutural nesta feature
├── ProxyPay.Infra/
│   ├── Context/
│   │   └── ProxyPayContext.cs        # DbContext único — OK (uma instância por requisição via factory)
│   ├── Repository/                   # Sem alteração
│   ├── AppServices/                  # AbacatePayAppService — será beneficiado pela propagação de tenant outbound
│   └── Migrations/                   # 5 migrações existentes — aplicadas por banco via runner cross-tenant
├── ProxyPay.Infra.Interfaces/        # Sem alteração estrutural
├── ProxyPay.GraphQL/                 # Schema admin; sem schema público (CLAUDE.md estava desatualizado)
├── ProxyPay.Tests/                   # xUnit — novos testes de isolamento e resolução de tenant
└── Lib/                              # NAuth, zTools, NTools (referência externa)

# A introduzir nesta feature (arquivos novos):
ProxyPay.API/Middlewares/
    TenantMiddleware.cs               # REESCRITO: rejeita quando header ausente/desconhecido
ProxyPay.Application/
    TenantCatalog.cs                  # Serviço que lista tenants ativos (lê appsettings)
    ITenantCatalog.cs                 # Interface correspondente
    TenantHeaderHandler.cs            # DelegatingHandler: propaga X-Tenant-Id outbound
ProxyPay.API/Controllers/
    WebhookController.cs              # ALTERADO: rota passa a incluir {tenantId} e middleware resolve
ProxyPay.Infra/Migrations/
    MigrationRunner.cs                # NOVO: aplica migrations em todos os tenants
ProxyPay.API/Tools/                   # NOVO (ou console separado): comando CLI "migrate-all-tenants"
```

**Structure Decision**: Mantém o layout Clean Architecture atual do
ProxyPay. Nenhum novo projeto é criado. Mudanças são localizadas em
`ProxyPay.API/Middlewares`, `ProxyPay.Application`,
`ProxyPay.API/Controllers/WebhookController.cs`, `ProxyPay.Infra/Migrations`
(runner cross-tenant) e nos quatro `appsettings*.json`. A arquitetura de
tenant segue 1:1 o padrão Lofn, já validado em produção.

## Complexity Tracking

Nenhuma violação da constituição. Tabela vazia intencionalmente — não há
justificativas de complexidade a registrar.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| _(nenhuma)_ | _(n/a)_ | _(n/a)_ |
