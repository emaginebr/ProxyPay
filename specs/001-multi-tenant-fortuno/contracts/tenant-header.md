# Contract: `X-Tenant-Id` Header

Regra única de identificação de tenant em requisições HTTP ao ProxyPay.
Aplicável a **toda** rota — autenticada, anônima, REST e GraphQL — exceto
as exempções listadas abaixo.

---

## Header canônico

```
X-Tenant-Id: {tenantId}
```

- **Obrigatório** em toda requisição que toque dados de negócio.
- Case-insensitive no nome do header (padrão HTTP); valor tratado como
  string opaca.
- Sem validação de formato — a validação é o "tenant resolvível pelo
  catálogo" (`ITenantCatalog.IsKnownTenant`).

## Rotas exemptadas

O middleware **não** exige o header nas seguintes rotas:

| Rota                                       | Motivo                                            |
|--------------------------------------------|---------------------------------------------------|
| `GET /`                                    | Health check                                      |
| `GET /swagger/*`                           | Documentação OpenAPI (apenas em Dev/Docker)       |
| `POST /webhook/abacatepay/{tenantId}`      | Tenant resolvido via path segment (ver contrato próprio) |

## Respostas de erro

### 400 — Header ausente

```
HTTP/1.1 400 Bad Request
Content-Type: application/json

{ "error": "tenant_context_required" }
```

Body opaco — não revela lista de tenants válidos, não revela qual parte
do request falhou. Mesma resposta para:
- Header completamente ausente.
- Header presente mas com valor vazio.
- Header presente com valor que não corresponde a nenhum tenant no catálogo.

### 401 — Token não autentica no tenant da requisição

Devolvido por NAuth quando:
- Token Basic inválido genericamente.
- Token Basic válido mas assinado com o JwtSecret de **outro** tenant.
- Token Basic expirado.

```
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{ "error": "unauthorized" }
```

As três causas **devem** produzir exatamente a mesma resposta (body e
headers) — FR-012 exige indistinguibilidade para bloquear enumeração de
tenants por oráculo.

### 404 — Recurso cross-tenant acessado por ID

Quando um usuário autenticado em tenant A tenta acessar um recurso por
ID pertencente a tenant B:

```
HTTP/1.1 404 Not Found
```

Resposta **idêntica** à de um ID inexistente dentro do próprio tenant.
Nenhum metadado do recurso alheio vaza.

---

## Exemplo de requisição válida

```
POST /store/insert HTTP/1.1
Host: api.proxypay.com
X-Tenant-Id: fortuno
Authorization: Basic <token-assinado-pelo-jwt-secret-de-fortuno>
Content-Type: application/json

{ "name": "Loja Teste", ... }
```

Respostas esperadas:
- `200 OK` — `Store` criado no banco de `fortuno`.
- `400 Bad Request` + `{"error":"tenant_context_required"}` — se o header
  faltar.
- `401 Unauthorized` — se o token for assinado por outro tenant.

---

## Propagação outbound

Ao realizar chamadas para serviços externos (`NAuth`, `zTools`,
`AbacatePay`) dentro do atendimento de uma requisição, o
`TenantHeaderHandler` (DelegatingHandler) automaticamente injeta o mesmo
valor `X-Tenant-Id` lido no inbound. Código de aplicação **não deve**
setar esse header manualmente nos `HttpClient` usados por `UserClient`,
`FileClient`, etc. — fica a cargo do handler.

---

## Testes de conformidade (obrigatórios para aceite)

1. `POST /store/insert` sem `X-Tenant-Id` → `400`.
2. `POST /store/insert` com `X-Tenant-Id: inexistente` → `400`.
3. `POST /graphql` sem `X-Tenant-Id` → `400` (mesmo sendo anônimo).
4. `GET /` sem `X-Tenant-Id` → `200 OK` (exempção de health check).
5. Token de `emagine` enviado com `X-Tenant-Id: fortuno` → `401` com body
   idêntico ao de token inválido genérico.
6. Chamada a `IUserClient.*` dentro de um handler autenticado como
   `fortuno` → o request outbound sai com `X-Tenant-Id: fortuno`.
