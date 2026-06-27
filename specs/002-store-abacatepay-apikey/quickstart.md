# Quickstart — API_KEY do AbacatePay por Loja

Verificação manual da feature após implementação. Pré-requisitos: API rodando (`dotnet run --project backend/ProxyPay.API`), banco do tenant migrado, token válido na coleção Bruno.

## 1. Definir a credencial de uma loja (Bruno)

Pasta `bruno/Store/` → request **Set ApiKey**:

```
PUT {{baseUrl}}/store/1/abacatepay-apikey
X-Tenant-Id: emagine
Authorization: Bearer {{token}}

{ "apiKey": "abc_dev_substitua_pela_chave_real" }
```

✅ Esperado: `204 No Content`, sem corpo.

## 2. Confirmar que a credencial NÃO é legível

### Via GraphQL (`POST {{baseUrl}}/graphql`)

```graphql
query { myStore { storeId name hasAbacatePayApiKey } }
```
✅ Esperado: `hasAbacatePayApiKey: true`.

```graphql
query { myStore { abacatepayApiKey } }
```
✅ Esperado: erro de validação — campo desconhecido (não existe no schema).

### Via REST

Não há endpoint REST que retorne a credencial. Conferir que nenhuma resposta de loja (criação/atualização) traz o campo.

## 3. Substituir a credencial

Repetir o request do passo 1 com outro valor ⇒ `204`. O valor anterior é descartado (não há histórico).

## 4. Validações de erro

| Cenário | Ação | Esperado |
|---------|------|----------|
| Vazia | `{ "apiKey": "" }` | `400` + mensagem; credencial anterior preservada |
| Loja inexistente | `PUT /store/999999/abacatepay-apikey` | `400` "Store not found" |
| Loja de outro usuário | `storeId` de outra loja | `403` |
| Sem token | remover `Authorization` | `401` |

## 5. Pagamento usa a credencial da loja

1. Loja **com** credencial: criar QR Code / Invoice / Billing (pasta `bruno/Payment/`) ⇒ chamada ao AbacatePay autenticada com a chave da loja; sucesso conforme a chave.
2. Loja **sem** credencial: tentar criar pagamento ⇒ `400` com erro claro (ex.: "Store has no AbacatePay credential configured"); **nenhuma** chamada ao provedor com chave de ambiente.

## 6. Checagem de segurança

- Inspecionar logs da API durante os passos 1 e 5: a `apiKey` **não** deve aparecer em nenhuma linha de log.
- Confirmar que respostas de erro não contêm o valor da credencial nem stack trace.

---

## ⚠️ Nota de migração (operacional)

Com a remoção do fallback para a chave global (FR-014), **as lojas em produção (emagine, monexup, fortuno) precisam cadastrar a credencial por loja antes de continuar transacionando**. Planejar:
1. Cadastrar a credencial de cada loja existente via passo 1 (uma vez por tenant/loja).
2. Só então remover/zerar a configuração global `AbacatePay:ApiKey` dos `appsettings`, se desejado.

Sem o passo 1, pagamentos das lojas sem credencial passarão a falhar com `400`.
