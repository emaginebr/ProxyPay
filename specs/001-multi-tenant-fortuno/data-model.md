# Phase 1 Data Model: Multi-Tenant "fortuno"

Esta feature **não introduz tabelas** nem colunas novas nos bancos de
negócio do ProxyPay. O isolamento é obtido por separação física de bancos
(um `ProxyPayContext` por tenant), não por coluna `tenant_id`. Este
documento modela os **artefatos de configuração e runtime** da camada de
tenant.

---

## Entidades runtime (em memória, não persistidas em tabela)

### `TenantDescriptor`

Representação imutável de um tenant ativo, lido do `appsettings`:

| Campo                      | Tipo       | Origem (config)                                          | Notas                                                                                  |
|----------------------------|------------|----------------------------------------------------------|----------------------------------------------------------------------------------------|
| `TenantId`                 | string     | chave do objeto (`Tenants:emagine` → `"emagine"`)         | String opaca, sem validação de formato (conforme clarificação Q4).                     |
| `ConnectionString`         | string     | `Tenants:{id}:ConnectionString`                          | PostgreSQL Npgsql; **NÃO deve aparecer em respostas de API**.                          |
| `JwtSecret`                | string     | `Tenants:{id}:JwtSecret`                                 | Usado pelo `NAuthTenantSecretProvider`; rotação = editar appsettings + reload config.   |
| `BucketName`               | string     | `Tenants:{id}:BucketName`                                | Bucket S3/blob para uploads via zTools `FileClient`.                                    |
| `AbacatePayWebhookSecret`  | string     | `Tenants:{id}:AbacatePayWebhookSecret`                   | **NOVO**: segredo de validação de webhook da AbacatePay; hoje vive em `AbacatePay:WebhookSecret` (global) e será movido por tenant. |
| `AbacatePayApiKey`         | string     | `Tenants:{id}:AbacatePayApiKey`                          | **NOVO**: chave de API para chamadas outbound à AbacatePay quando operando nesse tenant. Hoje é global em `AbacatePay:ApiKey`. |

### `ITenantCatalog` (novo, contrato)

Wrapper sobre `IConfiguration` que responde:
- `IEnumerable<string> GetActiveTenantIds()` — lê todas as chaves sob
  `Tenants:` em appsettings e retorna os IDs.
- `bool IsKnownTenant(string tenantId)` — verifica se o ID recebido no
  header existe no catálogo.
- `TenantDescriptor GetDescriptor(string tenantId)` — instancia o
  descriptor; lança `UnknownTenantException` se não existir.

Implementação `TenantCatalog` é singleton e mantém um cache interno
invalidado em reload de configuração (`IOptionsMonitor`).

### `ITenantContext` (já existente)

- `string TenantId { get; }` — retorna o tenant da requisição corrente;
  `null` quando não há `HttpContext` (ex.: durante runner de migrações).

### `ITenantResolver` (já existente)

- `string TenantId { get; }` — **alteração**: deixa de cair em
  `DefaultTenantId` quando o context está vazio. Em chamadas originadas
  fora de um escopo de requisição (runner), usa `TenantResolverScope` (ver
  abaixo).
- `string ConnectionString { get; }`
- `string JwtSecret { get; }`
- `string BucketName { get; }`

### `TenantResolverScope` (novo)

`IDisposable` que permite ao runner de migrações (ou a qualquer código
fora de HTTP) definir temporariamente o tenant sob o qual o
`TenantResolver` responderá. Implementação via `AsyncLocal<string>`
interno do `TenantContext`.

```csharp
using (tenantContext.EnterScope("fortuno"))
{
    // ITenantResolver.TenantId == "fortuno"
    context.Database.Migrate();
}
```

---

## Fluxo de resolução (por requisição)

```
HTTP request
 └─ UseRouting
    └─ UseCors
       └─ TenantMiddleware                  ★ (alterado)
          ├─ Rota em exemption list? (/, /swagger/*)
          │   └─ passa sem resolver tenant; rejeita se o restante do
          │     pipeline tentar acessar o DbContext (erro explícito)
          ├─ Rota é /webhook/abacatepay/{tenantId}?
          │   └─ lê tenant do path segment; valida via ITenantCatalog
          └─ caso geral:
              ├─ lê X-Tenant-Id do header
              ├─ valida via ITenantCatalog.IsKnownTenant
              ├─ ausente ou desconhecido → 400 com body opaco
              └─ ok → HttpContext.Items["TenantId"] = id
       └─ UseAuthentication  (NAuth valida com secret do tenant corrente)
          └─ UseAuthorization
             └─ Controller
                └─ ITenantResolver.ConnectionString →
                   TenantDbContextFactory → ProxyPayContext do tenant
                └─ HttpClient outbound:
                   TenantHeaderHandler injeta X-Tenant-Id
```

---

## Estado de configuração (appsettings)

### Antes (atual)

```json
{
  "Tenant": { "DefaultTenantId": "emagine" },
  "Tenants": {
    "emagine":  { "ConnectionString": "...", "JwtSecret": "...", "BucketName": "proxypay" },
    "monexup":  { "ConnectionString": "...", "JwtSecret": "...", "BucketName": "proxypay" }
  },
  "AbacatePay": {
    "ApiKey": "abc_dev_...",
    "WebhookSecret": "dev-webhook-secret"
  }
}
```

### Depois (alvo desta feature)

```json
{
  "Tenant": { "DefaultTenantId": "emagine" },   // mantido apenas para ferramentas offline
  "Tenants": {
    "emagine": {
      "ConnectionString": "Host=...;Database=proxypay_emagine;...",
      "JwtSecret": "***rotated-per-tenant***",
      "BucketName": "proxypay-emagine",
      "AbacatePayApiKey": "***",
      "AbacatePayWebhookSecret": "***"
    },
    "fortuno": {
      "ConnectionString": "Host=...;Database=proxypay_fortuno;...",
      "JwtSecret": "***rotated-per-tenant***",
      "BucketName": "proxypay-fortuno",
      "AbacatePayApiKey": "***",
      "AbacatePayWebhookSecret": "***"
    }
  },
  "AbacatePay": { "ApiUrl": "https://api.abacatepay.com" }
}
```

Notas:
- `monexup` removido (não faz parte da entrega).
- `AbacatePay.ApiKey` e `AbacatePay.WebhookSecret` **deixam de existir** no
  root — movidos para dentro de cada tenant.
- `AbacatePay.ApiUrl` continua global (é o endpoint do provedor, não um
  segredo por tenant).

---

## Identidade e ciclo de vida

- **TenantId**: não é validado quanto a formato; sua unicidade é garantida
  pelo YAML/JSON do catálogo (chaves duplicadas em JSON geram erro no
  parse).
- **Criação**: adicionar um objeto em `Tenants:{novoId}` e popular os
  segredos; recarregar config.
- **Alteração**: rotação de `JwtSecret` é suportada (FR-014); rotação da
  `ConnectionString` requer drenagem de conexões abertas.
- **Desativação / exclusão**: **fora de escopo** (clarificação Q3).

---

## Banco de dados (schema)

### Schema lógico por tenant

**Idêntico** ao schema atual do `ProxyPayContext`: `stores`, `customers`,
`invoices`, `invoice_items`, `billings`, `billing_items`, `transactions`.
Cinco migrations EF Core já existentes são aplicadas **por banco** via
runner cross-tenant.

Nenhum DDL é alterado nesta feature. Nenhuma coluna `tenant_id` é
adicionada.

### Bancos físicos (produção)

| Banco              | Tenant    | Origem                                            |
|--------------------|-----------|---------------------------------------------------|
| `proxypay_emagine` | `emagine` | Cópia integral do banco atual `proxypay_db`       |
| `proxypay_fortuno` | `fortuno` | Criado vazio; migrations aplicadas pelo runner    |

### Bancos físicos (desenvolvimento)

Recomendação — cada dev tem dois bancos locais separados:
`proxypay_emagine_dev` e `proxypay_fortuno_dev`. Appsettings.Development.json
é atualizado para apontar cada tenant ao banco próprio (hoje apontam ambos
ao mesmo banco, o que mascarava o bug de isolamento).

---

## Invariantes e validações

1. **Invariante de isolamento**: para qualquer par de tenants `(A, B)` com
   `A ≠ B`, as connection strings `Tenants:A:ConnectionString` e
   `Tenants:B:ConnectionString` devem apontar para **bancos físicos
   distintos**. Esta checagem é executada no startup da API: se duas
   entradas do catálogo compartilharem exatamente a mesma connection
   string, a aplicação falha rápida com mensagem clara (exceto em modo
   dev explicitamente anotado).
2. **Invariante de completude de tenant**: cada `TenantDescriptor` lido
   do catálogo deve ter todos os campos obrigatórios (`ConnectionString`,
   `JwtSecret`, `BucketName`). Ausência resulta em exceção no startup.
3. **Invariante do runner de migrations**: ao aplicar migrations, o
   runner itera `ITenantCatalog.GetActiveTenantIds()` e falha rápido no
   primeiro erro, com código de saída distinto por tipo (schema drift vs.
   connection error vs. unknown migration).
