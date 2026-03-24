# AbacatePay - Guia de Integração para Modelos de Linguagem (Atualizado e Sincronizado)

## Visão Geral

A AbacatePay é um gateway de pagamento que permite a criação e gestão de cobranças de forma eficiente.
Atualmente, aceita pagamentos via **PIX** e **Cartão**.
Outros métodos (boleto, crypto, etc.) poderão ser implementados futuramente.

## Autenticação

- **Método:** Bearer Token
- **Detalhes:** Todas as requisições à API devem incluir um token JWT no cabeçalho de autorização.
- **Exemplo de Cabeçalho:**
  Authorization: Bearer {SEU_TOKEN_AQUI}
- **Documentação:** [Authentication](https://docs.abacatepay.com/pages/authentication)

## Modo Desenvolvedor (Dev Mode)

- **Descrição:** Ambiente para testes e desenvolvimento. Todas as operações realizadas neste modo são simuladas e não afetam o ambiente de produção.
- **Documentação:** [Dev Mode](https://docs.abacatepay.com/pages/devmode)

---

## Endpoints Principais

### Clientes

#### ➤ Criar Cliente

- **Endpoint:** `POST /v1/customer/create`
- **curl esperado como exemplo:**

```bash
curl --request POST \
  --url https://api.abacatepay.com/v1/customer/create \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}' \
  --header 'content-type: application/json' \
  --data '{
    "name": "Fulano de tal",
    "cellphone": "(00) 0000-0000",
    "email": "cliente@gmail.com",
    "taxId": "123.456.789-01"
  }'
```

**Explicação de cada parâmetro do Body:**

- **name** (string, obrigatório): Nome completo do cliente. Exemplo: "Fulano de tal".
- **cellphone** (string, obrigatório): Telefone celular do cliente. Exemplo: "(00) 0000-0000".
- **email** (string, obrigatório): Endereço de e-mail do cliente. Exemplo: "cliente@gmail.com".
- **taxId** (string, obrigatório): CPF ou CNPJ válido do cliente. Exemplo: "123.456.789-01".

**Modelo de resposta:**

```json
{
  "data": {
    "id": "cust_abcdef123456",
    "metadata": {
      "name": "Fulano de tal",
      "cellphone": "(00) 0000-0000",
      "email": "cliente@gmail.com",
      "taxId": "123.456.789-01"
    }
  },
  "error": null
}
```

- **Documentação:** [Criar Cliente](https://docs.abacatepay.com/pages/client/create)

---

#### ➤ Listar Clientes

- **Endpoint:** `GET /v1/customer/list`
- **curl esperado como exemplo:**

```bash
curl --request GET \
  --url https://api.abacatepay.com/v1/customer/list \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}'
```

_Esta rota não necessita de body. Os parâmetros de autenticação via cabeçalho são obrigatórios._

**Modelo de resposta:**

```json
{
  "data": [
    {
      "id": "cust_abcdef123456",
      "metadata": {
        "name": "Fulano de tal",
        "cellphone": "(00) 0000-0000",
        "email": "cliente@gmail.com",
        "taxId": "123.456.789-01"
      }
    }
  ],
  "error": null
}
```

- **Documentação:** [Listar Clientes](https://docs.abacatepay.com/pages/client/list)

---

### Cupons de Desconto

#### ➤ Criar Cupom

- **Endpoint:** `POST /v1/coupon/create`
- **curl esperado como exemplo:**

```bash
curl --request POST \
  --url https://api.abacatepay.com/v1/coupon/create \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}' \
  --header 'content-type: application/json' \
  --data '{
    "data": {
      "code": "DEYVIN_20",
      "notes": "Cupom de desconto para meu público",
      "maxRedeems": 10,
      "discountKind": "PERCENTAGE",
      "discount": 20,
      "metadata": {}
    }
  }'
```

**Explicação de cada parâmetro do Body (dentro do objeto "data"):**

- **code** (string, obrigatório): Identificador único do cupom. Exemplo: "DEYVIN_20".
- **notes** (string): Descrição ou observação sobre o cupom. Exemplo: "Cupom de desconto para meu público".
- **maxRedeems** (number, obrigatório): Número máximo de vezes que o cupom pode ser resgatado. Exemplo: 10. Use `-1` para ilimitado.
- **discountKind** (string, obrigatório): Tipo de desconto, podendo ser "PERCENTAGE" ou "FIXED".
- **discount** (number, obrigatório): Valor de desconto a ser aplicado. Exemplo: 20.
- **metadata** (object, opcional): Objeto para incluir metadados adicionais do cupom.

**Modelo de resposta:**

```json
{
  "data": {
    "id": "DEYVIN_20",
    "notes": "Cupom de desconto para meu público",
    "maxRedeems": 10,
    "redeemsCount": 0,
    "discountKind": "PERCENTAGE",
    "discount": 20,
    "devMode": true,
    "status": "ACTIVE",
    "createdAt": "2025-05-25T23:43:25.250Z",
    "updatedAt": "2025-05-25T23:43:25.250Z",
    "metadata": {}
  },
  "error": null
}
```

- **Documentação:** https://docs.abacatepay.com/api-reference/criar-um-novo-cupom

---

#### ➤ Listar Cupons

- **Endpoint:** `GET /v1/coupon/list`
- **curl esperado como exemplo:**

```bash
curl --request GET \
  --url https://api.abacatepay.com/v1/coupon/list \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}'
```

_Esta rota não necessita de parâmetros no body._

**Modelo de resposta:**

```json
{
  "data": [
    {
      "id": "DEYVIN_20",
      "notes": "Cupom de desconto para meu público",
      "maxRedeems": -1,
      "redeemsCount": 0,
      "discountKind": "PERCENTAGE",
      "discount": 20,
      "devMode": true,
      "status": "ACTIVE",
      "createdAt": "2025-05-25T23:43:25.250Z",
      "updatedAt": "2025-05-25T23:43:25.250Z",
      "metadata": {}
    }
  ],
  "error": null
}
```

- **Documentação:** [Listar Cupons](https://docs.abacatepay.com/pages/payment/list)

---

### Cobranças

#### ➤ Criar Cobrança

- **Endpoint:** `POST /v1/billing/create`
- **curl esperado como exemplo:**

```bash
curl --request POST \
  --url https://api.abacatepay.com/v1/billing/create \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}' \
  --header 'content-type: application/json' \
  --data '{
    "frequency": "ONE_TIME",
    "methods": ["PIX","CARD"],
    "products": [
      {
        "externalId": "prod-1234",
        "name": "Assinatura de Programa Fitness",
        "description": "Acesso ao programa fitness premium por 1 mês.",
        "quantity": 2,
        "price": 2000
      }
    ],
    "returnUrl": "https://example.com/billing",
    "completionUrl": "https://example.com/completion",
    "customerId": "cust_abcdefghij"
  }'
```

**Explicação de cada parâmetro do Body:**

- **frequency** (string, obrigatório): Define o tipo de frequência da cobrança. Valores possíveis: `"ONE_TIME"` ou `"MULTIPLE_PAYMENTS"`.
- **methods** (array de string, obrigatório): Lista com os métodos de pagamento aceitos. Agora aceita `"PIX"` e `"CARD"`.
- **products** (array de objeto, obrigatório): Lista de produtos incluso na cobrança.
  - **externalId** (string, obrigatório): Identificador único do produto no seu sistema.
  - **name** (string, obrigatório): Nome do produto.
  - **description** (string): Descrição do produto.
  - **quantity** (integer, obrigatório, ≥1): Quantidade do produto.
  - **price** (integer, obrigatório, mínimo 100): Preço unitário em centavos.
- **returnUrl** (string, obrigatório - URI): URL para redirecionamento caso o cliente escolha a opção "Voltar".
- **completionUrl** (string, obrigatório - URI): URL para redirecionamento após a conclusão do pagamento.
- **customerId** (string, opcional): ID de um cliente já cadastrado.
- **customer** (object, opcional): Objeto contendo os dados do cliente para criação imediata.

**Modelo de resposta:**

```json
{
  "data": {
    "id": "bill_123456",
    "url": "https://pay.abacatepay.com/bill-5678",
    "amount": 4000,
    "status": "PENDING",
    "devMode": true,
    "methods": ["PIX","CARD"],
    "products": [
      {
        "id": "prod_123456",
        "externalId": "prod-1234",
        "quantity": 2
      }
    ],
    "frequency": "ONE_TIME",
    "nextBilling": null,
    "customer": {
      "id": "cust_abcdef123456",
      "metadata": {
        "name": "Fulano de tal",
        "cellphone": "(00) 0000-0000",
        "email": "cliente@gmail.com",
        "taxId": "123.456.789-01"
      }
    },
    "createdAt": "2025-03-24T21:50:20.772Z",
    "updatedAt": "2025-03-24T21:50:20.772Z"
  },
  "error": null
}
```

- **Documentação:** [Criar Cobrança](https://docs.abacatepay.com/pages/payment/create)

---

#### ➤ Buscar Cobrança

- **Endpoint:** `GET /v1/billing/get?id=bill_123456`
- **curl esperado como exemplo:**

```bash
curl --request GET \
  --url 'https://api.abacatepay.com/v1/billing/get?id=bill_123456' \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}'
```

**Modelo de resposta:** Igual ao modelo da criação de cobrança, retornando os detalhes de uma cobrança específica.

---

#### ➤ Listar Cobranças

- **Endpoint:** `GET /v1/billing/list`
- **curl esperado como exemplo:**

```bash
curl --request GET \
  --url https://api.abacatepay.com/v1/billing/list \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}'
```

**Modelo de resposta:**

```json
{
  "data": [
    {
      "id": "bill_123456",
      "url": "https://pay.abacatepay.com/bill-5678",
      "amount": 4000,
      "status": "PENDING",
      "devMode": true,
      "methods": ["PIX","CARD"],
      "products": [
        {
          "id": "prod_123456",
          "externalId": "prod-1234",
          "quantity": 2
        }
      ],
      "frequency": "ONE_TIME",
      "nextBilling": null,
      "customer": {
        "id": "cust_abcdef123456",
        "metadata": {
          "name": "Fulano de tal",
          "cellphone": "(00) 0000-0000",
          "email": "cliente@gmail.com",
          "taxId": "123.456.789-01"
        }
      }
    }
  ],
  "error": null
}
```

- **Documentação:** [Listar Cobranças](https://docs.abacatepay.com/pages/payment/list)

---

### PIX QRCode

#### ➤ Criar QRCode PIX

- **Endpoint:** `POST /v1/pixQrCode/create`
- **curl esperado como exemplo:**

```bash
curl --request POST \
  --url https://api.abacatepay.com/v1/pixQrCode/create \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}' \
  --header 'content-type: application/json' \
  --data '{
    "amount": 100,
    "expiresIn": 3600,
    "description": "Pagamento de serviço",
    "customer": {
      "name": "Fulano de tal",
      "cellphone": "(00) 0000-0000",
      "email": "cliente@gmail.com",
      "taxId": "123.456.789-01"
    },
    "metadata": {
      "teste": "Valor do teste de metadata"
    }
  }'
```

**Explicação de cada parâmetro do Body:**

- **amount** (number, obrigatório): Valor da cobrança em centavos. Exemplo: 100 (R$1,00).
- **expiresIn** (number, opcional): Tempo de expiração da cobrança em segundos. Exemplo: 3600 (1 hora).
- **description** (string, opcional, máximo 140 caracteres): Mensagem que será exibida durante o pagamento do PIX. Exemplo: "Pagamento de serviço".
- **customer** (object, opcional): Objeto contendo os dados do cliente para criação, caso este ainda não esteja cadastrado.
  - **name** (string, obrigatório caso customer seja passado): Nome do cliente.
  - **cellphone** (string, obrigatório caso customer seja passado): Telefone do cliente.
  - **email** (string, obrigatório caso customer seja passado): E-mail do cliente.
  - **taxId** (string, obrigatório caso customer seja passado): CPF ou CNPJ do cliente.
- **metadata** (object, opcional): Objeto contendo os dados de um metadata customizável por parte de quem está integrando.

**Modelo de resposta:**

```json
{
  "data": {
    "id": "pix_char_123456",
    "amount": 100,
    "status": "PENDING",
    "devMode": true,
    "brCode": "00020101021226950014br.gov.bcb.pix",
    "brCodeBase64": "data:image/png;base64,iVBORw0KGgoAAA",
    "platformFee": 80,
    "createdAt": "2025-03-24T21:50:20.772Z",
    "updatedAt": "2025-03-24T21:50:20.772Z",
    "expiresAt": "2025-03-25T21:50:20.772Z",
    "metadata": {
      "teste": "Valor do teste de metadata"
    }
  },
  "error": null
}
```

- **Documentação:** [Criar QRCode PIX](https://docs.abacatepay.com/pages/pix)

---

#### ➤ Checar Status do QRCode PIX

- **Endpoint:** `GET /v1/pixQrCode/check`
- **curl esperado como exemplo:**

```bash
curl --request GET \
  --url 'https://api.abacatepay.com/v1/pixQrCode/check?id=pix_char_123456' \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}'
```

_Esta rota utiliza um parâmetro na query:_

- **id** (string, obrigatório): ID do QRCode PIX. Exemplo: "pix_char_123456".

**Modelo de resposta:**

```json
{
  "data": {
    "status": "PENDING",
    "expiresAt": "2025-03-25T21:50:20.772Z"
  },
  "error": null
}
```

- **Documentação:** [Checar Status](https://docs.abacatepay.com/pages/pix)

---

#### ➤ Simular Pagamento do QRCode PIX (Somente em Dev Mode)

- **Endpoint:** `POST /v1/pixQrCode/simulate-payment`
- **curl esperado como exemplo:**

```bash
curl --request POST \
  --url 'https://api.abacatepay.com/v1/pixQrCode/simulate-payment?id=pix_char_123456' \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}' \
  --header 'content-type: application/json' \
  --data '{
    "metadata": {}
  }'
```

**Explicação de cada parâmetro:**

- **Query Parameter - id** (string, obrigatório): ID do QRCode PIX que terá o pagamento simulado.
- **No Body:**
  - **metadata** (object, opcional): Objeto para incluir dados adicionais sobre a simulação, se necessário.

**Modelo de resposta:**

```json
{
  "data": {
    "id": "pix_char_123456",
    "amount": 100,
    "status": "PAID",
    "devMode": true,
    "brCode": "00020101021226950014br.gov.bcb.pix",
    "brCodeBase64": "data:image/png;base64,iVBORw0KGgoAAA",
    "platformFee": 80,
    "createdAt": "2025-03-24T21:50:20.772Z",
    "updatedAt": "2025-03-24T21:50:20.772Z",
    "expiresAt": "2025-03-25T21:50:20.772Z"
  },
  "error": null
}
```

- **Documentação:** [Simular Pagamento](https://docs.abacatepay.com/pages/pix)

---

### Saques (Novo)

#### ➤ Criar Saque

- **Endpoint:** `POST /v1/withdraw/create`
- **curl esperado como exemplo:**

```bash
curl --request POST \
  --url https://api.abacatepay.com/v1/withdraw/create \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}' \
  --header 'content-type: application/json' \
  --data '{
    "amount": 5000,
    "pixKey": "fulano@banco.com",
    "notes": "Saque de teste"
  }'
```

**Explicação de cada parâmetro:**

- **amount** (number, obrigatório): valor do saque em centavos.
- **pixKey** (string, obrigatório): chave PIX do destinatário.
- **notes** (string, opcional): observação ou descrição do saque.

**Modelo de resposta:**

```json
{
  "data": {
    "id": "wd_123456",
    "amount": 5000,
    "status": "PENDING",
    "pixKey": "fulano@banco.com",
    "createdAt": "2025-03-24T21:50:20.772Z",
    "updatedAt": "2025-03-24T21:50:20.772Z"
  },
  "error": null
}
```

---

#### ➤ Buscar Saque

- **Endpoint:** `GET /v1/withdraw/get?id=wd_123456`
- **curl esperado como exemplo:**

```bash
curl --request GET \
  --url 'https://api.abacatepay.com/v1/withdraw/get?id=wd_123456' \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}'
```

**Modelo de resposta:** igual ao da criação.

---

#### ➤ Listar Saques

- **Endpoint:** `GET /v1/withdraw/list`
- **curl esperado como exemplo:**

```bash
curl --request GET \
  --url https://api.abacatepay.com/v1/withdraw/list \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}'
```

**Modelo de resposta:**

```json
{
  "data": [
    {
      "id": "wd_123456",
      "amount": 5000,
      "status": "PENDING",
      "pixKey": "fulano@banco.com",
      "createdAt": "2025-03-24T21:50:20.772Z",
      "updatedAt": "2025-03-24T21:50:20.772Z"
    }
  ],
  "error": null
}
```

---

### Loja (Novo)

#### ➤ Obter Detalhes da Loja

- **Endpoint:** `GET /v1/store/get`
- **curl esperado como exemplo:**

```bash
curl --request GET \
  --url https://api.abacatepay.com/v1/store/get \
  --header 'accept: application/json' \
  --header 'authorization: Bearer {SEU_TOKEN_AQUI}'
```

**Modelo de resposta:**

```json
{
  "data": {
    "id": "store_123456",
    "name": "Minha Loja",
    "createdAt": "2025-03-24T21:50:20.772Z"
  },
  "error": null
}
```

---

## Webhooks

- Notificações automáticas enviadas pela AbacatePay.
- Eventos disponíveis: `billing.paid`, `pix.paid`, `pix.expired`, `withdraw.paid`.
- Sempre validar a assinatura enviada.
- Implementar retries para lidar com falhas de rede.

---

## SDKs

- SDKs oficiais disponíveis para integração em linguagens populares.

---

## Transição para Produção

- **Descrição:** Para migrar do ambiente de desenvolvimento para produção, é necessário desativar o Dev Mode e completar o cadastro com informações adicionais.
- **Documentação:** [Produção](https://docs.abacatepay.com/pages/production)

---

_Este guia foi elaborado para auxiliar modelos de linguagem e desenvolvedores a integrar-se de forma eficaz com a API da AbacatePay utilizando os endpoints atualizados._