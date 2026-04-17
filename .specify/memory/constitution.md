<!--
Sync Impact Report
==================
Version change: 1.0.0 → 1.1.0

Rationale: MINOR bump — materially expands two existing sections without
removing or redefining any principle:
  (A) §V (Autenticação e Segurança) gains a bounded transitional exception
      to the CORS rule, keeping the base rule intact but acknowledging a
      known technical debt in the ProxyPay repo with an explicit expiry date.
  (B) §6 (Padrão de Tratamento de Erros) is expanded from a single
      required pattern to three accepted response patterns, legitimizing
      validation errors and webhook anti-retry silencing without weakening
      the "no stack traces / no secrets" invariant.

Both amendments were recommended by /speckit.analyze of feature
001-multi-tenant-fortuno (2026-04-17) as resolutions to constitution
conflicts CN1 and CN2 respectively.

Modified sections:
- §V. Autenticação e Segurança — CORS rule clarified with bounded exception
- §6 (Restrições Adicionais > Padrão de Tratamento de Erros) — three patterns recognized

Added sections:
- None.

Removed sections:
- None.

Templates requiring updates:
- ✅ .specify/memory/constitution.md               (this file — amended)
- ✅ .specify/templates/plan-template.md           (reviewed — Constitution Check
     gate continues to apply; no structural change needed)
- ✅ .specify/templates/spec-template.md           (reviewed — unaffected)
- ✅ .specify/templates/tasks-template.md          (reviewed — unaffected)
- ✅ .specify/templates/checklist-template.md      (reviewed — unaffected)
- ✅ .specify/templates/agent-file-template.md     (reviewed — unaffected)
- ⚠  .specify/templates/commands/*.md              (directory does not exist —
     no action required)
- ⚠  README.md / docs/quickstart.md                (not present at repo root —
     no action required; CLAUDE.md already carries runtime guidance)

Historical:
- 1.0.0 (2026-04-02) — Initial ratification; template fully materialized into
  five principles + Restrições Adicionais + Fluxo de Desenvolvimento.

Follow-up TODOs:
- CORS exception expires 2026-07-31 — before that date, `ProxyPay.API/Startup.cs`
  must be updated to restrict origins in `Docker` and `Production` environments.
  After expiry, the exception paragraph in §V must be removed (PATCH bump).
-->

# ProxyPay Constitution

## Core Principles

### I. Skills Obrigatórias

Todo trabalho de criação ou modificação de entidades, services, repositories,
DTOs, migrations e registro de DI no backend **DEVE** ser executado por meio da
skill `dotnet-architecture`. Essa skill encapsula a Clean Architecture adotada
pelos projetos .NET 8 (fluxo de dependências, repositórios genéricos,
mapeamento manual, DI centralizado, DbContext, Fluent API, migrações via
`dotnet ef`, nomeação de DTOs `Info` / `InsertInfo` / `Result` e chaves de
resposta em português `sucesso` / `mensagem` / `erros`).

Regras:

- Contribuidores **NÃO DEVEM** reimplementar manualmente padrões já cobertos
  pela skill — invocá-la é obrigatório sempre que o escopo do trabalho cair em
  sua descrição.
- Divergências do padrão da skill **DEVEM** ser justificadas em Complexity
  Tracking no plano da feature (ver `plan-template.md`).

**Rationale**: Centralizar os padrões em uma skill evita deriva arquitetural
entre contribuidores e reduz revisões repetitivas sobre estrutura e DI.

### II. Stack Tecnológica Bloqueada

A stack de backend é fixa e **NÃO DEVE** ser estendida com alternativas sem
emenda explícita desta constituição:

| Tecnologia            | Versão | Finalidade                             |
| --------------------- | ------ | -------------------------------------- |
| .NET                  | 8.0    | Runtime e framework principal          |
| Entity Framework Core | 9.x    | ORM e migrações                        |
| PostgreSQL            | Latest | Banco de dados relacional              |
| NAuth                 | Latest | Autenticação (Basic token)             |
| zTools                | Latest | Upload S3, e-mail (MailerSend), slugs  |
| Swashbuckle           | 8.x    | Swagger / OpenAPI                      |

Regras:

- ORMs alternativos (Dapper, NHibernate, etc.) **NÃO DEVEM** ser introduzidos —
  EF Core é o único ORM permitido.
- Comandos `docker` ou `docker compose` **NÃO DEVEM** ser executados no
  ambiente local; Docker não está acessível neste ambiente de desenvolvimento.
  Agentes automatizados e contribuidores **DEVEM** evitá-los mesmo quando
  pareçam a solução mais rápida.

**Rationale**: Uma stack fechada mantém a superfície operacional previsível
(migrations, observabilidade, deploy) e elimina fragmentação entre serviços.

### III. Convenções de Código (.NET)

Todo código C# **DEVE** seguir as convenções abaixo:

| Elemento          | Convenção          | Exemplo                                   |
| ----------------- | ------------------ | ----------------------------------------- |
| Namespaces        | PascalCase         | `ProxyPay.Domain.Services`                |
| Namespaces (decl) | File-scoped        | `namespace ProxyPay.API;`                 |
| Classes/Interfaces| PascalCase         | `CampaignService`, `ICampaignRepository`  |
| Métodos           | PascalCase         | `GetById()`, `MapToDto()`                 |
| Propriedades      | PascalCase         | `CampaignId`, `CreatedAt`                 |
| Campos privados   | `_camelCase`       | `_repository`, `_context`                 |
| Constantes        | `UPPER_CASE`       | `BUCKET_NAME`                             |

Regras adicionais:

- Todas as propriedades públicas de DTOs **DEVEM** declarar
  `[JsonPropertyName("camelCase")]` para manter payloads JSON em camelCase.
- Declarações de namespace **DEVEM** usar a forma file-scoped (`namespace X;`).

**Rationale**: Uniformidade de nomes reduz carga cognitiva em revisões e
alinha o contrato externo (JSON camelCase) ao padrão consumido pelo frontend.

### IV. Convenções de Banco de Dados (PostgreSQL)

O esquema PostgreSQL **DEVE** seguir estas regras:

| Elemento         | Convenção                     | Exemplo                              |
| ---------------- | ----------------------------- | ------------------------------------ |
| Tabelas          | snake_case plural             | `campaigns`, `campaign_entries`      |
| Colunas          | snake_case                    | `campaign_id`, `created_at`          |
| Primary Keys     | `{entidade}_id`, bigint IDENTITY | `campaign_id bigint PK`           |
| Constraint PK    | `{tabela}_pkey`               | `campaigns_pkey`                     |
| Foreign Keys     | `fk_{pai}_{filho}`            | `fk_campaign_entry`                  |
| Delete behavior  | `ClientSetNull`               | Cascade **NÃO É PERMITIDO**          |
| Timestamps       | `timestamp without time zone` | Sem timezone                         |
| Strings          | `varchar` com `MaxLength`     | `varchar(260)`                       |
| Booleans         | `boolean` com default         | `DEFAULT true`                       |
| Status/Enums     | `integer`                     | `DEFAULT 1`                          |

Regras:

- Configuração de DbContext, Fluent API e emissão de migrações via `dotnet ef`
  **DEVEM** seguir o detalhamento da skill `dotnet-architecture`.
- Exclusões em cascata **NÃO DEVEM** ser configuradas — comportamento padrão
  é `ClientSetNull` para proteger dados relacionados.

**Rationale**: Uma convenção única de nomeação/tipos evita surpresas em
migrações, queries cross-service e ferramentas de observabilidade.

### V. Autenticação e Segurança

A autenticação dos serviços **DEVE** seguir o padrão Basic via NAuth:

| Aspecto              | Padrão                                       |
| -------------------- | -------------------------------------------- |
| Esquema              | Basic Authentication via NAuth               |
| Header               | `Authorization: Basic {token}`               |
| Handler              | `NAuthHandler` registrado no DI              |
| Proteção de rotas    | Atributo `[Authorize]` nos controllers       |

Regras de segurança (inegociáveis):

- Connection strings, chaves e outros secrets **NUNCA DEVEM** ser retornados em
  respostas da API, logs públicos ou mensagens de erro expostas.
- Controllers que manipulem dados sensíveis (stores, products, orders, images,
  storeusers, etc.) **DEVEM** carregar o atributo `[Authorize]`.
- CORS com `AllowAnyOrigin` **SOMENTE** é permitido no ambiente `Development`.
  Nos ambientes `Docker` e `Production` a origem **DEVE** ser restringida.

**Exceção transitória (até 2026-07-31)**: o repositório `ProxyPay` mantém
`AllowAnyOrigin` em todos os ambientes como dívida técnica explicitamente
reconhecida nesta constituição. Trabalho de remediação (CORS condicional por
ambiente em `ProxyPay.API/Startup.cs`) **DEVE** ser concluído antes de
2026-07-31. Ao expirar, esta exceção **DEVE** ser removida desta seção via
emenda PATCH. Enquanto a exceção estiver vigente, toda PR que toque o
pipeline de CORS **DEVE** documentar se aproxima ou afasta o repositório da
conformidade final.

**Rationale**: Padronizar autenticação em um único handler concentra auditoria
e reduz o risco de rotas desprotegidas ou CORS permissivo em produção. A
exceção transitória existe para evitar "silent ignoring" da regra em
features não-relacionadas: o débito fica rastreável, datado e visível em
todo gate de `/speckit.analyze`.

## Restrições Adicionais

### Variáveis de Ambiente

As variáveis abaixo **DEVEM** estar definidas em todos os ambientes suportados:

| Variável                                      | Obrigatória | Descrição                                    |
| --------------------------------------------- | ----------- | -------------------------------------------- |
| `ConnectionStrings__<nome-do-projeto>Context` | Sim         | Connection string PostgreSQL                 |
| `ASPNETCORE_ENVIRONMENT`                      | Sim         | `Development`, `Docker` ou `Production`      |

Segredos **NÃO DEVEM** ser commitados em `appsettings*.json` versionados; usar
variáveis de ambiente ou `.env` fora do controle de versão.

### Padrão de Tratamento de Erros

Controllers **DEVEM** envolver lógica suscetível a falha em um bloco
try/catch que retorne **uma** das três respostas-padrão abaixo, escolhida
conforme a natureza do endpoint e do erro:

| # | Resposta                          | Quando usar                                                                                       |
|---|-----------------------------------|---------------------------------------------------------------------------------------------------|
| 1 | `BadRequest(message)` (`400`)      | Falha de validação, pré-condição do cliente, payload inválido, entidade não encontrada pelo input.|
| 2 | `StatusCode(500, ex.Message)`      | **Fallback genérico** para qualquer erro não-classificado da lógica interna.                      |
| 3 | `Ok()` (`200`) com log interno     | Handlers de webhook de provedores externos onde retentativa automática do provedor é indesejada.  |

Exemplos:

```csharp
// Padrão 2 — fallback genérico
try { /* lógica */ }
catch (Exception ex) { return StatusCode(500, ex.Message); }

// Padrão 3 — webhook anti-retry-storm (AbacatePay, etc.)
try { /* processa payload externo */ }
catch (Exception ex) { _logger.LogError(ex, "webhook processing error"); return Ok(); }
```

Regras invariantes (aplicam-se às três variantes):

- Respostas de erro **NÃO DEVEM** incluir stack traces completas nem
  detalhes internos de infraestrutura (connection strings, caminhos de
  arquivo, nomes de host internos).
- Segredos (JwtSecret, chaves de API, senhas) **NUNCA DEVEM** aparecer em
  `message` nem em `ex.Message` retornado ao cliente — sanitizar antes se
  necessário.
- Quando um webhook retorna `Ok()` mascarando falha, o erro **DEVE** ser
  registrado em log estruturado (`ILogger.LogError`) com contexto
  suficiente para diagnóstico offline.
- Escolher o Padrão 1 quando a falha é atribuível ao cliente; o Padrão 2
  quando é atribuível ao servidor; o Padrão 3 exclusivamente em rotas
  cujo consumidor externo re-tenta automaticamente em caso de 4xx/5xx.

**Rationale**: A regra original reconhecia apenas o fallback 500, o que
obrigava controllers de validação e handlers de webhook externos a
divergir silenciosamente. Enumerar explicitamente os três padrões aceitos
elimina a ambiguidade sem relaxar as garantias de segurança (nenhum dos
três permite stack trace ou vazamento de segredo).

## Fluxo de Desenvolvimento e Qualidade

### Checklist para Novos Contribuidores

Antes de submeter qualquer código, o contribuidor **DEVE** confirmar:

- [ ] Utilizou a skill `dotnet-architecture` para toda nova entidade backend.
- [ ] Tabelas e colunas no PostgreSQL seguem `snake_case`.
- [ ] Controllers que expõem dados sensíveis possuem `[Authorize]`.
- [ ] DTOs usam `[JsonPropertyName("camelCase")]` nas propriedades públicas.
- [ ] Nenhuma connection string ou secret é retornada em respostas de API.
- [ ] Nenhum comando `docker` / `docker compose` foi executado localmente.

### Portões de Qualidade

- **Constitution Check** (ver `.specify/templates/plan-template.md`): toda
  feature **DEVE** passar no gate antes do Phase 0 e ser reavaliada após o
  Phase 1. Violações **DEVEM** ser registradas em Complexity Tracking com
  justificativa e alternativa mais simples descartada.
- **Revisão de código**: PRs **DEVEM** ser revisados contra os Princípios
  I–V desta constituição antes do merge.
- **Migrações**: toda migração EF Core **DEVE** ser gerada via `dotnet ef`
  (não editada manualmente) e revisada quanto a `ClientSetNull` e naming.

## Governance

Esta constituição **SUBSTITUI** quaisquer práticas informais ou convenções
tácitas conflitantes dentro do repositório. Em caso de colisão entre este
documento e outro arquivo (incluindo `CLAUDE.md`, READMEs, comentários de
código), esta constituição prevalece até ser emendada.

### Procedimento de Emenda

1. Abrir proposta de emenda descrevendo (a) princípio/seção afetado, (b)
   motivação, (c) impacto em templates e código existente.
2. Se a emenda for aceita, aplicar via `/speckit.constitution`, que
   **DEVE** propagar mudanças para os templates em `.specify/templates/` e
   emitir um Sync Impact Report no topo deste arquivo.
3. Atualizar `CONSTITUTION_VERSION` seguindo SemVer:
   - **MAJOR**: remoção ou redefinição incompatível de princípio/governança.
   - **MINOR**: adição de princípio/seção ou expansão material de regra.
   - **PATCH**: correções de texto, clarificações, ajustes não-semânticos.
4. Registrar a data de `LAST_AMENDED_DATE` (ISO `YYYY-MM-DD`) no rodapé.

### Conformidade e Revisão

- Toda PR **DEVE** declarar, implícita ou explicitamente (via Constitution
  Check do plano), conformidade com os princípios inegociáveis.
- Complexidade adicional **DEVE** ser justificada em Complexity Tracking.
- Guia de desenvolvimento em tempo de execução (modelos mentais, comandos,
  estrutura de pastas) permanece em `CLAUDE.md`; convenções inegociáveis
  permanecem aqui.

**Version**: 1.1.0 | **Ratified**: 2026-04-02 | **Last Amended**: 2026-04-17
