# API Reference - ProxyPay Backend

> Documentação completa da API REST e GraphQL do backend ProxyPay. Inclui todos os endpoints, DTOs, enums e estruturas de dados.

**Created:** 2026-03-18
**Last Updated:** 2026-03-26

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

#### ProxyPaySetting

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `apiUrl` | `string` | URL base da API |
| `bucketName` | `string` | Nome do bucket de armazenamento |

---

## Enums

### InvoiceStatusEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `Pending` | Pendente (padrão ao criar) |
| `2` | `Sent` | Enviado ao cliente |
| `3` | `Paid` | Pago (preenche `paidAt` automaticamente) |
| `4` | `Overdue` | Vencido |
| `5` | `Cancelled` | Cancelado |
| `6` | `Expired` | Expirado |

### PaymentMethodEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `Pix` | Pagamento via Pix |
| `2` | `Boleto` | Pagamento via Boleto |
| `3` | `CreditCard` | Cartão de crédito |
| `4` | `DebitCard` | Cartão de débito |

### TransactionTypeEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `Credit` | Crédito (entrada) |
| `2` | `Debit` | Débito (saída) |

### TransactionCategoryEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `Payment` | Pagamento |
| `2` | `Refund` | Reembolso |
| `3` | `Adjustment` | Ajuste |
| `4` | `Fee` | Taxa |
| `5` | `Withdrawal` | Saque |
| `6` | `Deposit` | Depósito |

### BillingStatusEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `AwaitingPayment` | Aguardando pagamento |
| `2` | `Active` | Ativa |
| `3` | `Suspended` | Suspensa |
| `4` | `Cancelled` | Cancelada |

### BillingFrequencyEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `Monthly` | Mensal |
| `2` | `Quarterly` | Trimestral |
| `3` | `Semiannual` | Semestral |
| `4` | `Annual` | Anual |

### BillingStrategyEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `Immediate` | Cobrança imediata |
| `2` | `FirstDayOfMonth` | Cobrança no primeiro dia do mês |

---

## GraphQL API

A API expõe o endpoint GraphQL autenticado via HotChocolate em `/graphql/admin`, com suporte a **offset-based pagination**, **projection**, **filtering** e **sorting**.

Playground interativo (Banana Cake Pop) disponível em `https://localhost:44374/graphql/admin/`.

### Configuração Global

| Item | Valor |
|------|-------|
| **Max page size** | `50` |
| **Default page size** | `10` |
| **Total count** | Sempre incluído |
| **Max field cost** | `8000` |

### Paginação (Offset-Based)

Todas as queries paginadas suportam os seguintes argumentos:

| Argumento | Tipo | Default | Descrição |
|-----------|------|---------|-----------|
| `skip` | `Int` | `0` | Quantidade de registros a pular |
| `take` | `Int` | `10` | Quantidade de registros a retornar (máx: 50) |

O retorno é envelopado em um tipo `CollectionSegment`:

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `items` | `[T]` | Lista de itens da página atual |
| `pageInfo` | `CollectionSegmentInfo` | Informações de navegação (`hasNextPage`, `hasPreviousPage`) |
| `totalCount` | `Int` | Total de registros (sempre disponível) |

---

### Queries Disponíveis

Todas as queries requerem Bearer Token via header `Authorization`.

| Query | Retorno | Paginação | Filtering | Sorting | Descrição |
|-------|---------|-----------|-----------|---------|-----------|
| `myStore` | `Store` | Não | Não | Não | Loja do usuário autenticado |
| `myInvoices(skip, take)` | `InvoiceCollectionSegment` | Sim | Sim | Sim | Invoices da loja do usuário |
| `myInvoiceByNumber(invoiceNumber: String!)` | `Invoice` | Não | Não | Não | Invoice pelo número |
| `myTransactions(skip, take)` | `TransactionCollectionSegment` | Sim | Sim | Sim | Transações da loja do usuário |
| `myBalance` | `BalanceSummary` | Não | Não | Não | Resumo financeiro da loja |
| `myCustomers(skip, take)` | `CustomerCollectionSegment` | Sim | Sim | Sim | Clientes da loja do usuário |
| `myBillings(skip, take)` | `BillingCollectionSegment` | Sim | Sim | Sim | Cobranças recorrentes da loja |

---

### Tipos GraphQL — Campos Disponíveis

#### Store

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `storeId` | `Long!` | ID da loja |
| `clientId` | `String!` | Identificador do cliente (máx. 32 chars) |
| `userId` | `Long!` | ID do proprietário |
| `name` | `String!` | Nome da loja (máx. 240 chars) |
| `email` | `String` | Email da loja (máx. 240 chars) |
| `billingStrategy` | `Int!` | Estratégia de cobrança (ver `BillingStrategyEnum`, default: 1) |
| `createdAt` | `DateTime!` | Data de criação |
| `updatedAt` | `DateTime!` | Data da última atualização |
| `invoices` | `[Invoice]` | Relação: invoices da loja |
| `transactions` | `[Transaction]` | Relação: transações da loja |
| `customers` | `[Customer]` | Relação: clientes da loja |
| `billings` | `[Billing]` | Relação: cobranças recorrentes da loja |

#### Customer

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `customerId` | `Long!` | ID do cliente |
| `storeId` | `Long` | ID da loja (nullable) |
| `name` | `String!` | Nome do cliente (máx. 240 chars) |
| `documentId` | `String` | CPF/CNPJ (máx. 20 chars) |
| `cellphone` | `String` | Celular (máx. 20 chars) |
| `email` | `String` | Email (máx. 240 chars) |
| `createdAt` | `DateTime!` | Data de criação |
| `updatedAt` | `DateTime!` | Data da última atualização |
| `store` | `Store` | Relação: loja do cliente |
| `invoices` | `[Invoice]` | Relação: invoices do cliente |
| `billings` | `[Billing]` | Relação: cobranças do cliente |

#### Invoice

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `invoiceId` | `Long!` | ID do invoice |
| `customerId` | `Long` | ID do cliente (nullable) |
| `storeId` | `Long` | ID da loja (nullable) |
| `invoiceNumber` | `String!` | Número único do invoice (máx. 50 chars) |
| `notes` | `String` | Observações |
| `status` | `Int!` | Status (ver `InvoiceStatusEnum`, default: 1) |
| `paymentMethod` | `Int!` | Método de pagamento (ver `PaymentMethodEnum`, default: 1) |
| `discount` | `Float!` | Desconto geral (default: 0) |
| `dueDate` | `DateTime!` | Data de vencimento |
| `externalCode` | `String` | Código externo (máx. 240 chars) |
| `expiresAt` | `DateTime` | Data de expiração (nullable) |
| `paidAt` | `DateTime` | Data de pagamento (nullable, preenchido ao marcar como Paid) |
| `createdAt` | `DateTime!` | Data de criação |
| `updatedAt` | `DateTime!` | Data da última atualização |
| `customer` | `Customer` | Relação: cliente do invoice |
| `store` | `Store` | Relação: loja do invoice |
| `items` | `[InvoiceItem]` | Relação (type extension): itens do invoice |

#### InvoiceItem

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `invoiceItemId` | `Long!` | ID do item |
| `invoiceId` | `Long!` | ID do invoice pai |
| `description` | `String!` | Descrição do item (máx. 500 chars) |
| `quantity` | `Int!` | Quantidade |
| `unitPrice` | `Float!` | Preço unitário |
| `discount` | `Float!` | Desconto do item (default: 0) |
| `total` | `Float!` | Total calculado: (quantity * unitPrice) - discount |
| `createdAt` | `DateTime!` | Data de criação |
| `invoice` | `Invoice` | Relação: invoice pai |

#### Transaction

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `transactionId` | `Long!` | ID da transação |
| `invoiceId` | `Long` | ID do invoice relacionado (nullable) |
| `storeId` | `Long` | ID da loja (nullable) |
| `type` | `Int!` | Tipo (ver `TransactionTypeEnum`) |
| `category` | `Int!` | Categoria (ver `TransactionCategoryEnum`) |
| `description` | `String!` | Descrição da transação (máx. 500 chars) |
| `amount` | `Float!` | Valor da transação |
| `balance` | `Float!` | Saldo após a transação |
| `createdAt` | `DateTime!` | Data de criação |
| `store` | `Store` | Relação: loja da transação |
| `invoice` | `Invoice` | Relação (type extension): invoice relacionado (nullable) |

#### Billing

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `billingId` | `Long!` | ID da cobrança |
| `storeId` | `Long` | ID da loja (nullable) |
| `customerId` | `Long` | ID do cliente (nullable) |
| `frequency` | `Int!` | Frequência (ver `BillingFrequencyEnum`) |
| `paymentMethod` | `Int!` | Método de pagamento (ver `PaymentMethodEnum`, default: 1) |
| `billingStartDate` | `DateTime!` | Data de início da cobrança |
| `status` | `Int!` | Status (ver `BillingStatusEnum`, default: 1) |
| `createdAt` | `DateTime!` | Data de criação |
| `store` | `Store` | Relação: loja |
| `customer` | `Customer` | Relação: cliente |
| `items` | `[BillingItem]` | Relação: itens da cobrança |

#### BillingItem

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `billingItemId` | `Long!` | ID do item |
| `billingId` | `Long!` | ID da cobrança pai |
| `description` | `String!` | Descrição do item (máx. 500 chars) |
| `quantity` | `Int!` | Quantidade |
| `unitPrice` | `Float!` | Preço unitário |
| `discount` | `Float!` | Desconto do item (default: 0) |
| `createdAt` | `DateTime!` | Data de criação |
| `billing` | `Billing` | Relação: cobrança pai |

#### BalanceSummary

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `balance` | `Float!` | Saldo atual (último balance de transação) |
| `totalCredits` | `Float!` | Soma de todas as transações de crédito |
| `totalDebits` | `Float!` | Soma de todas as transações de débito |
| `transactionCount` | `Int!` | Total de transações |

---

### Exemplos de Consulta

#### Obter minha loja

```graphql
{
  myStore {
    storeId
    clientId
    name
    email
    billingStrategy
    createdAt
    updatedAt
  }
}
```

#### Listar meus clientes (com paginação)

```graphql
{
  myCustomers(skip: 0, take: 10) {
    items {
      customerId
      name
      documentId
      cellphone
      email
      createdAt
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
    totalCount
  }
}
```

#### Listar meus invoices com itens e cliente

```graphql
{
  myInvoices(skip: 0, take: 10) {
    items {
      invoiceId
      invoiceNumber
      status
      paymentMethod
      discount
      dueDate
      expiresAt
      paidAt
      createdAt
      updatedAt
      customer {
        customerId
        name
        email
      }
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

#### Buscar invoice por número

```graphql
{
  myInvoiceByNumber(invoiceNumber: "INV-0001-000001") {
    invoiceId
    invoiceNumber
    status
    paymentMethod
    discount
    dueDate
    paidAt
    customer {
      name
      email
    }
    items {
      description
      quantity
      unitPrice
      discount
      total
    }
  }
}
```

#### Listar minhas transações

```graphql
{
  myTransactions(skip: 0, take: 20) {
    items {
      transactionId
      type
      category
      description
      amount
      balance
      createdAt
      invoice {
        invoiceId
        invoiceNumber
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

#### Obter meu saldo

```graphql
{
  myBalance {
    balance
    totalCredits
    totalDebits
    transactionCount
  }
}
```

#### Listar minhas cobranças recorrentes com itens

```graphql
{
  myBillings(skip: 0, take: 10) {
    items {
      billingId
      frequency
      paymentMethod
      billingStartDate
      status
      createdAt
      customer {
        customerId
        name
        email
      }
      items {
        billingItemId
        description
        quantity
        unitPrice
        discount
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

### Filtering e Sorting

Todos os campos escalares suportam filtering e sorting via argumentos gerados automaticamente pelo HotChocolate.

#### Filtering — Operadores disponíveis

| Operador | Descrição | Exemplo |
|----------|-----------|---------|
| `eq` | Igual | `{ status: { eq: 3 } }` |
| `neq` | Diferente | `{ status: { neq: 5 } }` |
| `gt` | Maior que | `{ amount: { gt: 100.0 } }` |
| `gte` | Maior ou igual | `{ createdAt: { gte: "2026-01-01" } }` |
| `lt` | Menor que | `{ amount: { lt: 50.0 } }` |
| `lte` | Menor ou igual | `{ dueDate: { lte: "2026-12-31" } }` |
| `in` | Contido em lista | `{ status: { in: [1, 2] } }` |
| `nin` | Não contido em lista | `{ status: { nin: [5] } }` |
| `contains` | Contém (string) | `{ name: { contains: "João" } }` |
| `startsWith` | Começa com (string) | `{ invoiceNumber: { startsWith: "INV-0001" } }` |
| `endsWith` | Termina com (string) | `{ email: { endsWith: "@gmail.com" } }` |
| `and` | Combina condições (AND) | `{ and: [{ status: { eq: 3 } }, { amount: { gt: 100 } }] }` |
| `or` | Combina condições (OR) | `{ or: [{ status: { eq: 1 } }, { status: { eq: 2 } }] }` |

#### Exemplo — Filtrar invoices pagos

```graphql
{
  myInvoices(where: { status: { eq: 3 } }) {
    items {
      invoiceId
      invoiceNumber
      paidAt
      items {
        description
        total
      }
    }
    totalCount
  }
}
```

#### Exemplo — Filtrar transações de crédito acima de R$ 100

```graphql
{
  myTransactions(where: { and: [{ type: { eq: 1 } }, { amount: { gt: 100.0 } }] }) {
    items {
      transactionId
      description
      amount
      balance
      createdAt
    }
    totalCount
  }
}
```

#### Exemplo — Buscar clientes por nome

```graphql
{
  myCustomers(where: { name: { contains: "Silva" } }) {
    items {
      customerId
      name
      email
      cellphone
    }
    totalCount
  }
}
```

#### Exemplo — Filtrar cobranças ativas mensais

```graphql
{
  myBillings(where: { and: [{ status: { eq: 2 } }, { frequency: { eq: 1 } }] }) {
    items {
      billingId
      billingStartDate
      customer {
        name
      }
      items {
        description
        unitPrice
        quantity
      }
    }
    totalCount
  }
}
```

#### Exemplo — Sorting por data de criação (mais recente primeiro)

```graphql
{
  myInvoices(order: { createdAt: DESC }) {
    items {
      invoiceId
      invoiceNumber
      status
      createdAt
    }
    totalCount
  }
}
```

#### Exemplo — Sorting por valor + paginação

```graphql
{
  myTransactions(skip: 0, take: 10, order: { amount: DESC }) {
    items {
      transactionId
      description
      amount
      balance
    }
    pageInfo {
      hasNextPage
    }
    totalCount
  }
}
```

#### Exemplo — Combinar filtering + sorting + paginação

```graphql
{
  myInvoices(
    skip: 0
    take: 5
    where: { status: { in: [1, 2] } }
    order: { dueDate: ASC }
  ) {
    items {
      invoiceId
      invoiceNumber
      status
      dueDate
      customer {
        name
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

## Referências Externas

### UserInfo (NAuth.DTO)

DTO externo do pacote NAuth. Contém dados do usuário autenticado (ID, nome, email, etc.). Utilizado internamente para validação de sessão.

---

## Resumo

| Recurso | Endpoints REST | DTOs |
|---------|----------------|------|
| **Store** | 4 (insert, update, uploadLogo, delete) | 3 (StoreInfo, StoreInsertInfo, StoreUpdateInfo) |
| **Invoice** | 5 (insert, update, list, getById, delete) | 4 (InvoiceInfo, InvoiceInsertInfo, InvoiceUpdateInfo, InvoiceItemInsertInfo) |
| **GraphQL** | 1 endpoint, 7 queries | 8 tipos (Store, Customer, Invoice, InvoiceItem, Transaction, Billing, BillingItem, BalanceSummary) |
| **Enums** | — | 7 (InvoiceStatus, PaymentMethod, TransactionType, TransactionCategory, BillingStatus, BillingFrequency, BillingStrategy) |

- **Endpoint GraphQL autenticado:** `/graphql/admin` (myStore, myInvoices, myInvoiceByNumber, myTransactions, myBalance, myCustomers, myBillings)
- **Todos os endpoints REST e GraphQL requerem Bearer Token**
- **Serialização JSON:** propriedades em `camelCase` via `[JsonPropertyName]`
- **GraphQL:** suporte a paginação offset-based, projection, filtering e sorting via HotChocolate
