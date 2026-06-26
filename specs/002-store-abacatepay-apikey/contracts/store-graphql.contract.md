# Contrato GraphQL — Tipo `Store`

A query autenticada `myStore` (em `AdminQuery`) retorna `Store`. Este contrato garante que a credencial do AbacatePay **não** seja consultável e que exista um indicador booleano de configuração.

## Schema resultante (campo `Store`)

```graphql
type Store {
  storeId: Long!
  clientId: String
  userId: Long!
  name: String!
  email: String
  billingStrategy: Int!
  createdAt: DateTime!
  updatedAt: DateTime!
  hasAbacatePayApiKey: Boolean!   # NOVO — indicador; true se a credencial está configurada
  # abacatepayApiKey            -> AUSENTE do schema (Ignore()); não é consultável
}
```

> O campo `abacatepayApiKey` **não existe** no schema. Qualquer query que o solicite resulta em erro de validação do GraphQL (campo desconhecido) — não há caminho de leitura do valor (FR-006).

## Configuração de tipo (alvo)

`ProxyPay.GraphQL/Types/StoreType.cs`:

```csharp
public class StoreType : ObjectType<Store>
{
    protected override void Configure(IObjectTypeDescriptor<Store> descriptor)
    {
        descriptor.Field(s => s.AbacatePayApiKey).Ignore();

        descriptor.Field("hasAbacatePayApiKey")
            .Type<NonNullType<BooleanType>>()
            .Resolve(ctx =>
            {
                var parent = ctx.Parent<Store>();
                var context = ctx.Service<ProxyPayContext>();
                return context.Stores
                    .Where(s => s.StoreId == parent.StoreId)
                    .Select(s => s.AbacatePayApiKey != null && s.AbacatePayApiKey != "")
                    .FirstOrDefault();
            });
    }
}
```

Registro em `GraphQLServiceExtensions.AddProxyPayGraphQL`:

```csharp
.AddQueryType<AdminQuery>()
.AddType<StoreType>()                 // NOVO
.AddTypeExtension<InvoiceTypeExtension>()
.AddTypeExtension<TransactionTypeExtension>()
```

## Exemplos

Consulta válida:
```graphql
query { myStore { storeId name hasAbacatePayApiKey } }
```
Resposta (loja com credencial):
```json
{ "data": { "myStore": { "storeId": 1, "name": "My Store", "hasAbacatePayApiKey": true } } }
```

Consulta inválida (rejeitada pelo schema):
```graphql
query { myStore { abacatepayApiKey } }   # erro: campo desconhecido
```

## Invariantes de teste (aceite)

1. `myStore` com `hasAbacatePayApiKey` ⇒ retorna `true`/`false` corretamente conforme a loja tenha ou não credencial. (US2 #1, FR-013)
2. Solicitar `abacatepayApiKey` ⇒ erro de validação do schema (campo inexistente). (US2 #1, FR-006)
3. Introspecção do tipo `Store` ⇒ não lista nenhum campo de credencial além do booleano. (FR-005)
