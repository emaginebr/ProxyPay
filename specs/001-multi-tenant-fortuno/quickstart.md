# Quickstart: Multi-Tenant "fortuno"

Roteiro mínimo para (a) subir localmente os dois tenants, (b) executar o
fluxo fim-a-fim, e (c) preparar a promoção para produção.

---

## Pré-requisitos

- .NET 8 SDK instalado
- PostgreSQL acessível localmente (sem Docker, conforme constituição)
- `dotnet-ef` CLI instalado: `dotnet tool install --global dotnet-ef`
- Usuário PostgreSQL com permissão de `CREATE DATABASE`

---

## 1. Criar os bancos locais

Executar, uma vez, no `psql` ou equivalente:

```sql
CREATE DATABASE proxypay_emagine_dev;
CREATE DATABASE proxypay_fortuno_dev;
```

Ambos ficam vazios. As migrations serão aplicadas no passo 3.

---

## 2. Configurar `appsettings.Development.json`

Substituir a seção `Tenants` por:

```json
"Tenants": {
  "emagine": {
    "ConnectionString": "Host=localhost;Port=5432;Database=proxypay_emagine_dev;Username=proxypay_user;Password=pikpro6",
    "JwtSecret": "<gerar-base64-256bit-para-emagine>",
    "BucketName": "proxypay-emagine-dev",
    "AbacatePayApiKey": "abc_dev_Ggm5xKQhnNyfEtMg2K466j4D",
    "AbacatePayWebhookSecret": "dev-webhook-secret-emagine"
  },
  "fortuno": {
    "ConnectionString": "Host=localhost;Port=5432;Database=proxypay_fortuno_dev;Username=proxypay_user;Password=pikpro6",
    "JwtSecret": "<gerar-base64-256bit-para-fortuno>",
    "BucketName": "proxypay-fortuno-dev",
    "AbacatePayApiKey": "abc_dev_Ggm5xKQhnNyfEtMg2K466j4D",
    "AbacatePayWebhookSecret": "dev-webhook-secret-fortuno"
  }
}
```

Remover o tenant `monexup` se ainda estiver presente. Remover as chaves
globais `AbacatePay.ApiKey` e `AbacatePay.WebhookSecret` (movidas para
dentro de cada tenant).

Para gerar JwtSecret distintos:
```bash
# PowerShell / pwsh
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
```

---

## 3. Aplicar migrations nos dois bancos

A partir da branch `001-multi-tenant-fortuno`, com o runner já
implementado:

```bash
dotnet run --project ProxyPay.API -- --migrate-all-tenants
```

Saída esperada:
```
[emagine] applying 5 migrations to proxypay_emagine_dev ... OK (5 applied, duration=1.2s)
[fortuno] applying 5 migrations to proxypay_fortuno_dev ... OK (5 applied, duration=0.9s)
MigrateAllTenants: 2/2 tenants succeeded.
```

(Antes da implementação do runner, usar fallback manual:
`dotnet ef database update --project ProxyPay.Infra --startup-project
ProxyPay.API --connection "<cs-do-tenant>"`, uma vez para cada tenant.)

---

## 4. Subir a API

```bash
dotnet run --project ProxyPay.API
```

API disponível em `https://localhost:44374`.

---

## 5. Smoke test manual

### 5.1. Health check (sem tenant)

```bash
curl -k https://localhost:44374/
```
Esperado: `200 OK` com JSON `statusApplication: Healthy`.

### 5.2. Requisição sem header de tenant → 400

```bash
curl -k -X POST https://localhost:44374/graphql \
  -H 'Content-Type: application/json' \
  -d '{"query":"{ __typename }"}'
```
Esperado: `400` com `{"error":"tenant_context_required"}`.

### 5.3. Requisição com tenant desconhecido → 400

```bash
curl -k -X POST https://localhost:44374/graphql \
  -H 'X-Tenant-Id: inexistente' \
  -H 'Content-Type: application/json' \
  -d '{"query":"{ __typename }"}'
```
Esperado: `400` (mesma resposta opaca).

### 5.4. Requisição em tenant válido → 200

```bash
curl -k -X POST https://localhost:44374/graphql \
  -H 'X-Tenant-Id: fortuno' \
  -H 'Content-Type: application/json' \
  -d '{"query":"{ __typename }"}'
```
Esperado: `200`.

### 5.5. Isolamento verificado

1. Autenticar em `emagine` (obter token Basic emitido com segredo de
   emagine).
2. Criar um `Store` via `POST /store/insert`.
3. Autenticar em `fortuno`.
4. Tentar `GET` do `Store` criado (mesmo ID) → `404`.
5. Listar stores em `fortuno` → lista não contém o store criado em
   `emagine`.

### 5.6. Cross-tenant token → 401

1. Token Basic emitido no contexto `emagine`.
2. `POST /store/insert` com `X-Tenant-Id: fortuno` + esse token.
3. Esperado: `401`, body idêntico ao de token expirado.

---

## 6. Promoção para produção

Procedimento de migração da base atual (`proxypay_db`) → tenant
`emagine`, conforme R-008 do `research.md`:

1. Agendar janela de manutenção.
2. Parar o tráfego da API em produção.
3. Criar os bancos: `CREATE DATABASE proxypay_emagine;` e
   `CREATE DATABASE proxypay_fortuno;`
4. Dump + restore:
   ```bash
   pg_dump --no-owner --no-acl proxypay_db \
     | psql proxypay_emagine
   ```
5. Validar rowcounts: diff por tabela entre `proxypay_db` e
   `proxypay_emagine` deve ser zero.
6. Atualizar env vars de produção com as connection strings novas + os
   `JwtSecret` e `AbacatePayWebhookSecret` por tenant.
7. Registrar as URLs de webhook no painel da AbacatePay para cada tenant
   (ver contracts/webhook-tenant-routing.md).
8. Deploy do build da branch `001-multi-tenant-fortuno`.
9. Executar o runner:
   ```bash
   dotnet ProxyPay.API.dll --migrate-all-tenants
   ```
10. Reabrir tráfego.
11. Sanity check fim-a-fim: usuário real em `emagine` faz login → lista
    stores → cria invoice/QR code → paga → webhook retorna.
12. Sanity check de criação em `fortuno`: criar usuário/store do zero.

### Rollback plan

Se algo falhar entre passos 8 e 11:
1. Parar tráfego.
2. Reverter env vars para apontar `ConnectionString` do `emagine` de volta
   para `proxypay_db` (banco antigo, intocado).
3. Re-deploy do build anterior (`main` antes desta feature).
4. Reabrir tráfego.
O banco `proxypay_emagine` recém-criado fica como evidência para análise
post-mortem.

---

## 7. Troubleshooting

- **`tenant_context_required` em chamadas que costumavam funcionar**: o
  cliente parou de enviar `X-Tenant-Id`. Atualizar o cliente.
- **`401` em tokens que estavam válidos**: o JwtSecret foi rotacionado ou
  o token foi emitido sob outro tenant. Re-login no tenant correto.
- **Startup falha com `InvalidOperationException: Duplicate connection
  string across tenants`**: dois tenants no catálogo apontam para o mesmo
  banco físico. Corrigir connection strings antes de subir.
- **Migration runner para no primeiro erro**: ler o log estruturado; o
  tenant que falhou é impresso com a migration em andamento. Migrations
  já aplicadas não precisam de rollback — apenas corrigir o tenant
  problemático e re-rodar (idempotente).
