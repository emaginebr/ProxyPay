# Quickstart: Executar a suíte de testes de API

**Feature**: 003-api-endpoint-tests | **Date**: 2026-06-26

Guia para configurar e rodar `ProxyPay.ApiTests` localmente. A suíte **assume a API já em execução** (não a sobe).

## Pré-requisitos

1. **API ProxyPay em execução** numa URL conhecida (ex.: `https://localhost:44374` ou `http://localhost:5000`), configurada para o sandbox/DevMode da AbacatePay.
2. **NAuth API** acessível para login (a mesma URL usada pelo frontend em `VITE_NAUTH_API_URL`).
3. Um **usuário de teste** válido (email/senha) e o **tenant** correspondente (`X-Tenant-Id`).
4. O **segredo do webhook** AbacatePay configurado na API de teste.
5. O **ClientId** de uma store de teste para os fluxos de pagamento.
6. .NET 8 SDK instalado.

## 1. Build

```powershell
cd backend
dotnet build ProxyPay.ApiTests/
```

## 2. Fornecer segredos via variáveis de ambiente

Nenhum segredo fica em `appsettings.Test.json` (apenas placeholders `REPLACE_VIA_ENV_*`). A fixture falha rápido nomeando a env var faltante se algum placeholder não for resolvido.

PowerShell:

> A URL do NAuth **deve usar `https`** — em `http` o servidor redireciona e o POST de login vira GET (405). Os testes de pagamento **criam uma loja nova por teste** e setam nela a `Store:AbacatePayApiKey`, então **não há `Store:ClientId`** a configurar.

PowerShell:

```powershell
$env:ApiBaseUrl        = 'http://localhost:5002'
$env:Auth__BaseUrl     = 'https://emagine.com.br/auth-api'   # NAuth API (https!)
$env:Auth__Email       = 'qa@proxypay.test'
$env:Auth__Password    = '<senha>'
$env:Auth__Tenant      = 'emagine'                           # X-Tenant-Id
$env:Auth__LoginEndpoint = '/user/loginWithEmail'
$env:Store__AbacatePayApiKey = '<api-key-abacatepay-sandbox>'
$env:Webhook__Secret   = '<segredo-webhook-abacatepay>'
```

Bash:

```bash
export ApiBaseUrl=http://localhost:5002
export Auth__BaseUrl=https://emagine.com.br/auth-api
export Auth__Email=qa@proxypay.test
export Auth__Password=<senha>
export Auth__Tenant=emagine
export Auth__LoginEndpoint=/user/loginWithEmail
export Store__AbacatePayApiKey=<api-key-abacatepay-sandbox>
export Webhook__Secret=<segredo-webhook-abacatepay>
```

## 3. Rodar a suíte

```powershell
cd backend
dotnet test ProxyPay.ApiTests/
```

Toda a suíte (FR-004/SC-004) roda com um único comando. Para um arquivo específico:

```powershell
dotnet test ProxyPay.ApiTests/ --filter "FullyQualifiedName~PaymentControllerTests"
```

## 4. Repetibilidade (SC-006)

Os testes são idempotentes (R5): podem rodar duas vezes seguidas sem limpeza e produzir o mesmo resultado. Não dependem de banco/tenant limpo.

## CI (GitHub Actions) — esboço

Adicione os segredos em *Settings → Secrets and variables → Actions* e referencie-os:

```yaml
- name: API tests
  env:
    ApiBaseUrl: ${{ vars.API_BASE_URL }}
    Auth__BaseUrl: ${{ vars.NAUTH_BASE_URL }}
    Auth__Email: ${{ secrets.QA_EMAIL }}
    Auth__Password: ${{ secrets.QA_PASSWORD }}
    Auth__Tenant: ${{ vars.QA_TENANT }}
    Store__ClientId: ${{ vars.QA_STORE_CLIENTID }}
    Webhook__Secret: ${{ secrets.ABACATEPAY_WEBHOOK_SECRET }}
  run: dotnet test backend/ProxyPay.ApiTests/ --logger "trx;LogFileName=api-tests.trx"
```

## Solução de problemas

| Sintoma | Causa provável | Ação |
|---------|----------------|------|
| Exceção citando `REPLACE_VIA_ENV_...` no startup | env var não exportada | Exportar a variável indicada e rodar de novo |
| Falha de login NAuth (status do login) | URL/endpoint/credenciais incorretos | Conferir `Auth__BaseUrl`, `Auth__LoginEndpoint`, `Auth__Email/Password` |
| `401` em endpoints autenticados | token não aplicado como Basic / tenant ausente | Confirmar header `Authorization: Basic` + `X-Tenant-Id` na fixture |
| Testes de pagamento `400` inesperado | `Store__ClientId` inválido para o tenant | Usar ClientId de store existente no tenant de teste |
| Webhook não reflete efeito | sandbox não determinístico | Asserts focam contrato/`200`, não estado — ver R3/R5 |
