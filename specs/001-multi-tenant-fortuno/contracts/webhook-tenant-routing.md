# Contract: AbacatePay Webhook Tenant Routing

AbacatePay (provedor externo de pagamento) dispara webhooks para o
ProxyPay. Provedores externos não enviam cabeçalhos customizados, então
o tenant é resolvido via **path segment**, não via header.

---

## Endpoint

```
POST /webhook/abacatepay/{tenantId}?secret={webhookSecret}
```

- `{tenantId}` **obrigatório** no path; valida-se contra `ITenantCatalog`.
- `{webhookSecret}` obrigatório na query string; valida-se contra
  `Tenants:{tenantId}:AbacatePayWebhookSecret` do tenant resolvido pelo path.
- **Nenhum `X-Tenant-Id` é esperado** — o middleware detecta o prefixo da
  rota e injeta `HttpContext.Items["TenantId"]` a partir do path.

## Provisionamento do tenant junto à AbacatePay

Para cada tenant ativo, o operador registra manualmente no painel da
AbacatePay:

| Tenant   | Webhook URL                                                |
|----------|------------------------------------------------------------|
| emagine  | `https://api.proxypay.com/webhook/abacatepay/emagine`      |
| fortuno  | `https://api.proxypay.com/webhook/abacatepay/fortuno`      |

Cada URL recebe seu próprio `WebhookSecret` (visível apenas no painel) e
deve ser gravado em `Tenants:{id}:AbacatePayWebhookSecret` no appsettings.

## Respostas

### 200 OK (sempre — política defensiva do AbacatePay)

Mesmo quando o secret é inválido ou o payload é malformado, a resposta é
`200 OK` para evitar o retry storm do provedor. O erro é registrado em
log estruturado e ignorado silenciosamente.

```
HTTP/1.1 200 OK
```

### 404 Not Found

Quando `{tenantId}` no path **não existe** no catálogo. Este caso é
diferente do secret inválido — aqui o tenant é estruturalmente
desconhecido, o que indica configuração incorreta do provedor e não uma
falha transitória:

```
HTTP/1.1 404 Not Found
```

(Comportamento defensivo: retornar 404 faz o AbacatePay marcar a URL como
inválida e parar retries — efeito desejado quando a config está errada.)

---

## Fluxo interno

```
POST /webhook/abacatepay/fortuno?secret=***
  └─ TenantMiddleware detecta prefixo /webhook/abacatepay/
     ├─ extrai {tenantId} = "fortuno" do path
     ├─ ITenantCatalog.IsKnownTenant("fortuno") → true
     └─ HttpContext.Items["TenantId"] = "fortuno"
  └─ WebhookController.AbacatePayWebhook
     ├─ valida secret contra Tenants:fortuno:AbacatePayWebhookSecret
     ├─ IInvoiceService.ProcessWebhookAsync (roda no DbContext de fortuno)
     └─ retorna 200
```

---

## Testes de conformidade

1. Webhook para `/webhook/abacatepay/fortuno` com secret válido do
   `fortuno` → `200`, invoice do `fortuno` é atualizado.
2. Webhook para `/webhook/abacatepay/fortuno` com secret válido do
   `emagine` → `200`, log de warning "invalid secret", nada é atualizado.
3. Webhook para `/webhook/abacatepay/desconhecido` → `404`.
4. Webhook sem o path segment (rota `/webhook/abacatepay`) → `404`
   (MapControllers não encontra rota).
5. Invoice ID presente em `emagine` chega via webhook de `fortuno` →
   nenhum registro de `emagine` é alterado (isolamento físico de banco
   garante).
