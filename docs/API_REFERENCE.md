# API Reference - Ganesha Backend

> Documentação completa da API REST e GraphQL do backend Ganesha. Inclui todos os endpoints, DTOs, enums e estruturas de dados.

**Created:** 2026-03-18
**Last Updated:** 2026-03-22

---

## Informações Gerais

| Item | Valor |
|------|-------|
| **Base URL (Dev)** | `https://localhost:44374` |
| **Autenticação** | Bearer Token via header `Authorization` |
| **Content-Type** | `application/json` (exceto upload de imagens) |
| **Respostas de Erro** | `400` Bad Request, `401` Unauthorized, `404` Not Found, `500` Internal Server Error |

---

## Autenticação

Todos os endpoints (exceto os marcados como **Público**) exigem o header:

```
Authorization: Bearer <token>
```

O token é validado via `NAuth`. Caso inválido ou ausente, retorna `401 Unauthorized`.

---

## Endpoints

### 1. Store Controller

**Prefixo:** `/store`

> **Nota:** Os endpoints de leitura (`list`, `listActive`, `getBySlug`, `getById`) foram migrados para o GraphQL. Use as queries `stores`, `storeBySlug` e `myStores` nos endpoints `/graphql` e `/graphql/admin`.

#### POST `/insert` — Criar loja

- **Auth:** Requerida
- **Request Body:** `StoreInsertInfo`
- **Response:** `StoreInfo`
- **Descrição:** Cria a loja com status Active por padrão

#### POST `/update` — Atualizar loja

- **Auth:** Requerida
- **Request Body:** `StoreUpdateInfo`
- **Response:** `StoreInfo`

#### POST `/uploadLogo/{storeId}` — Upload de logomarca

- **Auth:** Requerida
- **Content-Type:** `multipart/form-data`
- **Params:** `storeId` (long)
- **Body:** `file` (IFormFile, máx. 100MB)
- **Response:** `StoreInfo`

#### DELETE `/delete/{storeId}` — Deletar loja

- **Auth:** Requerida
- **Params:** `storeId` (long)
- **Response:** `204 No Content`

---

### 2. Invoice Controller

**Prefixo:** `/invoice`

#### POST `/insert` — Criar invoice

- **Auth:** Requerida
- **Request Body:** `InvoiceInsertInfo`
- **Response:** `InvoiceInfo`
- **Descrição:** Cria o invoice com status `Draft`, gera número automático (`INV-{userId}-{seq}`), calcula subtotal e total automaticamente a partir dos itens. Deve conter pelo menos 1 item.

#### POST `/update` — Atualizar invoice

- **Auth:** Requerida
- **Request Body:** `InvoiceUpdateInfo`
- **Response:** `InvoiceInfo`
- **Descrição:** Atualiza o invoice e substitui todos os itens. Recalcula subtotal e total. Se o status for alterado para `Paid`, o campo `paidAt` é preenchido automaticamente.

#### GET `/list` — Listar invoices do usuário

- **Auth:** Requerida
- **Response:** `IList<InvoiceInfo>`
- **Descrição:** Retorna todos os invoices do usuário autenticado, ordenados por data de criação (mais recente primeiro). Cada invoice inclui seus itens.

#### GET `/getById/{invoiceId}` — Obter invoice por ID

- **Auth:** Requerida
- **Params:** `invoiceId` (long)
- **Response:** `InvoiceInfo`
- **Descrição:** Retorna o invoice com todos os seus itens. Retorna `404` se não encontrado. Retorna `403` se o invoice não pertence ao usuário.

#### DELETE `/delete/{invoiceId}` — Deletar invoice

- **Auth:** Requerida
- **Params:** `invoiceId` (long)
- **Response:** `200 OK`
- **Descrição:** Remove o invoice e todos os seus itens (cascade).

---

## DTOs (Data Transfer Objects)

### Invoice

#### InvoiceInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `invoiceId` | `long` | ID do invoice |
| `invoiceNumber` | `string` | Número gerado automaticamente (ex: `INV-0001-000001`) |
| `notes` | `string` | Observações |
| `status` | `InvoiceStatusEnum` | Status do invoice |
| `subTotal` | `double` | Soma dos totais dos itens |
| `discount` | `double` | Desconto geral (default: 0) |
| `tax` | `double` | Imposto (default: 0) |
| `total` | `double` | Valor final (subTotal - discount + tax) |
| `dueDate` | `DateTime` | Data de vencimento |
| `paidAt` | `DateTime?` | Data de pagamento (preenchido automaticamente ao marcar como Paid) |
| `createdAt` | `DateTime` | Data de criação |
| `updatedAt` | `DateTime` | Data da última atualização |
| `items` | `InvoiceItemInfo[]` | Lista de itens do invoice |

#### InvoiceInsertInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `notes` | `string` | Observações |
| `discount` | `double` | Desconto geral |
| `tax` | `double` | Imposto |
| `dueDate` | `DateTime` | Data de vencimento |
| `items` | `InvoiceItemInsertInfo[]` | Itens do invoice (obrigatório, mínimo 1) |

#### InvoiceUpdateInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `invoiceId` | `long` | ID do invoice (obrigatório) |
| `notes` | `string` | Observações |
| `status` | `InvoiceStatusEnum` | Novo status |
| `discount` | `double` | Desconto geral |
| `tax` | `double` | Imposto |
| `dueDate` | `DateTime` | Data de vencimento |
| `items` | `InvoiceItemInsertInfo[]` | Novos itens (substituem os anteriores, obrigatório, mínimo 1) |

#### InvoiceItemInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `invoiceItemId` | `long` | ID do item |
| `invoiceId` | `long` | ID do invoice pai |
| `description` | `string` | Descrição do item |
| `quantity` | `int` | Quantidade |
| `unitPrice` | `double` | Preço unitário |
| `discount` | `double` | Desconto do item (default: 0) |
| `total` | `double` | Total do item (quantity * unitPrice - discount) |
| `createdAt` | `DateTime` | Data de criação |

#### InvoiceItemInsertInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `description` | `string` | Descrição do item |
| `quantity` | `int` | Quantidade |
| `unitPrice` | `double` | Preço unitário |
| `discount` | `double` | Desconto do item |

---

### Store

#### StoreInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `storeId` | `long` | ID da loja |
| `slug` | `string` | Slug da loja |
| `name` | `string` | Nome da loja |
| `ownerId` | `long` | ID do proprietário |
| `logo` | `string` | Nome do arquivo da logomarca |
| `logoUrl` | `string` | URL completa da logomarca |
| `status` | `StoreStatusEnum` | Status da loja |

#### StoreInsertInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `name` | `string` | Nome da loja |

#### StoreUpdateInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `storeId` | `long` | ID da loja |
| `name` | `string` | Novo nome |
| `status` | `StoreStatusEnum` | Novo status |

---

### Settings

#### GaneshaSetting

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `apiUrl` | `string` | URL base da API |
| `bucketName` | `string` | Nome do bucket de armazenamento |

---

## Enums

### InvoiceStatusEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `Draft` | Rascunho (padrão ao criar) |
| `2` | `Sent` | Enviado ao cliente |
| `3` | `Paid` | Pago (preenche `paidAt` automaticamente) |
| `4` | `Overdue` | Vencido |
| `5` | `Cancelled` | Cancelado |

### StoreStatusEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `0` | `Inactive` | Loja inativa |
| `1` | `Active` | Loja ativa (padrão) |
| `2` | `Suspended` | Loja suspensa |

---

## GraphQL API

A API expõe dois endpoints GraphQL via HotChocolate, ambos com suporte a **offset-based pagination**, **projection**, **filtering** e **sorting**.

#### Paginação (Offset-Based)

Todas as queries que retornam listas suportam paginação offset-based com os seguintes argumentos:

| Argumento | Tipo | Default | Descrição |
|-----------|------|---------|-----------|
| `skip` | `Int` | `0` | Quantidade de registros a pular |
| `take` | `Int` | `10` | Quantidade de registros a retornar (máx: 50) |

O retorno é envelopado em um tipo `CollectionSegment` com a seguinte estrutura:

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `items` | `[T]` | Lista de itens da página atual |
| `pageInfo` | `CollectionSegmentInfo` | Informações de navegação (`hasNextPage`, `hasPreviousPage`) |
| `totalCount` | `Int` | Total de registros (sempre disponível) |

> **Nota:** A query `storeBySlug` não utiliza paginação pois retorna um único registro.

### Endpoint Público: `/graphql`

Playground interativo (Banana Cake Pop) disponível em `https://localhost:44374/graphql/`.

Não requer autenticação. Expõe apenas dados ativos/públicos.

#### Queries disponíveis

| Query | Retorno | Descrição |
|-------|---------|-----------|
| `stores(skip, take)` | `StoreCollectionSegment` | Lojas ativas (`status = 1`) |
| `storeBySlug(slug: String!)` | `[Store]` | Loja ativa pelo slug (sem paginação) |

#### Campos ocultos no schema público

O tipo `Store` no endpoint público **não expõe**: `ownerId`.

#### Exemplo

```graphql
{
  stores(skip: 0, take: 10) {
    items {
      storeId
      name
      slug
      logoUrl
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
    totalCount
  }
}
```

---

### Endpoint Autenticado: `/graphql/admin`

Requer Bearer Token via header `Authorization`. Retorna `401` se ausente ou inválido.

#### Queries disponíveis

| Query | Retorno | Descrição |
|-------|---------|-----------|
| `myStores(skip, take)` | `StoreCollectionSegment` | Lojas do usuário autenticado (filtradas por `ownerId`) |
| `myInvoices(skip, take)` | `InvoiceCollectionSegment` | Invoices do usuário autenticado (filtrados por `userId`) |

#### Exemplo — Listar minhas lojas

```graphql
{
  myStores(skip: 0, take: 10) {
    items {
      storeId
      name
      slug
      logoUrl
      status
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
    totalCount
  }
}
```

#### Exemplo — Listar meus invoices com itens

```graphql
{
  myInvoices(skip: 0, take: 10) {
    items {
      invoiceId
      invoiceNumber
      status
      subTotal
      discount
      tax
      total
      dueDate
      paidAt
      createdAt
      items {
        invoiceItemId
        description
        quantity
        unitPrice
        discount
        total
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
    totalCount
  }
}
```

---

### Tipos GraphQL

Os tipos GraphQL mapeiam diretamente as entidades do banco de dados:

| Tipo | Campos principais | Campos computados | Relações navegáveis |
|------|-------------------|-------------------|---------------------|
| `Store` | `storeId`, `slug`, `name`, `logo`, `status`, `ownerId`* | `logoUrl` | — |
| `Invoice` | `invoiceId`, `userId`, `invoiceNumber`, `notes`, `status`, `subTotal`, `discount`, `tax`, `total`, `dueDate`, `paidAt`, `createdAt`, `updatedAt` | — | `items` (via type extension) |
| `InvoiceItem` | `invoiceItemId`, `invoiceId`, `description`, `quantity`, `unitPrice`, `discount`, `total`, `createdAt` | — | `invoice` |

> \* `ownerId` é **oculto** no schema público (`/graphql`), visível apenas no admin (`/graphql/admin`).

### Filtering e Sorting

Todos os campos escalares suportam filtering e sorting via argumentos gerados automaticamente pelo HotChocolate.

**Exemplo de filtering:**
```graphql
{
  myInvoices(where: { status: { eq: PAID } }) {
    items {
      invoiceId
      invoiceNumber
      total
      paidAt
    }
    totalCount
  }
}
```

**Exemplo de sorting:**
```graphql
{
  myInvoices(order: { total: DESC }) {
    items {
      invoiceId
      invoiceNumber
      total
    }
    totalCount
  }
}
```

---

## Referências Externas

### UserInfo (NAuth.DTO)

DTO externo do pacote NAuth. Contém dados do usuário autenticado (ID, nome, email, etc.). Utilizado internamente para validação de sessão.

---

## Resumo

| Recurso | Endpoints REST | DTOs |
|---------|----------------|------|
| **Store** | 4 (insert, update, uploadLogo, delete) | 4 (inclui enum) |
| **Invoice** | 5 (insert, update, list, getById, delete) | 6 (inclui enum e item DTOs) |
| **GraphQL** | 2 endpoints, 4 queries | 3 tipos + 1 campo computado |
| **Total** | **9 REST + 2 GraphQL** | **13** |

- **Endpoints GraphQL públicos:** `/graphql` (stores, storeBySlug)
- **Endpoints GraphQL autenticados:** `/graphql/admin` (myStores, myInvoices)
- **Todos os endpoints REST requerem Bearer Token**
- **Serialização JSON:** propriedades em `camelCase` via `[JsonPropertyName]`
- **Leituras migradas para GraphQL:** listagem de stores e invoices são feitas via GraphQL, com suporte a paginação offset-based, projection, filtering e sorting
