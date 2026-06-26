# Contrato REST — Alterar credencial AbacatePay da loja

Endpoint dedicado e exclusivo para definir/substituir a credencial do AbacatePay de uma loja. É o **único** caminho de escrita da credencial. Não existe endpoint de leitura correspondente.

## Requisição

```
PUT /store/{storeId}/abacatepay-apikey
```

| Aspecto | Valor |
|---------|-------|
| Autenticação | `[Authorize]` (Basic token via NAuth) — obrigatória |
| Header | `X-Tenant-Id: {tenant}` |
| Header | `Authorization: Bearer {token}` |
| Rota | `storeId` (long) — loja alvo |
| Corpo | `application/json` — `StoreApiKeyUpdateInfo` |

### Corpo

```json
{
  "apiKey": "abc_live_xxxxxxxxxxxxxxxxxxxxxxxx"
}
```

## Respostas

| Status | Quando | Corpo |
|--------|--------|-------|
| `204 No Content` | Credencial definida/substituída com sucesso | — (nenhum eco do valor) |
| `400 Bad Request` | `apiKey` vazia/ausente; ou loja inexistente | `string` mensagem clara (sem secret, sem stack trace) |
| `401 Unauthorized` | Sem sessão/autenticação válida | — |
| `403 Forbidden` | Loja não pertence ao usuário autenticado | — |

> A resposta **nunca** retorna o valor da credencial — sucesso é `204` sem corpo (FR-005/FR-006).

## Assinatura do controller (alvo)

```csharp
[HttpPut("{storeId}/abacatepay-apikey")]
public async Task<ActionResult> SetAbacatePayApiKey(long storeId, [FromBody] StoreApiKeyUpdateInfo info)
{
    var userSession = _userClient.GetUserInSession(HttpContext);
    if (userSession == null)
        return Unauthorized();

    try
    {
        await _storeService.UpdateAbacatePayApiKeyAsync(storeId, info.ApiKey, userSession.UserId);
        return NoContent();
    }
    catch (UnauthorizedAccessException)
    {
        return Forbid();
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
```

## Invariantes de teste (aceite)

1. `PUT` com `apiKey` válida em loja própria sem credencial ⇒ `204`; loja passa a ter credencial (verificável via `hasAbacatePayApiKey` no GraphQL). (US1 #1)
2. `PUT` com `apiKey` diferente ⇒ `204`; valor anterior substituído. (US1 #2)
3. `PUT` em loja de outro usuário ⇒ `403`; nada gravado. (US1 #3)
4. `PUT` sem token ⇒ `401`. (US1 #4)
5. `PUT` com `apiKey: ""` ⇒ `400`; credencial anterior preservada. (FR-008)
6. `PUT` com `storeId` inexistente ⇒ `400`. (FR-009)
7. Nenhuma resposta (incluindo erros) contém o valor da credencial. (FR-005)
