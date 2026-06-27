namespace ProxyPay.DTO.GraphQL
{
    public class GraphQLRequest
    {
        /// <summary>GraphQL query string</summary>
        /// <example>{ myStores { items { storeId name } } }</example>
        public string Query { get; set; }

        /// <summary>Operation name (optional, for multi-operation documents)</summary>
        public string OperationName { get; set; }

        /// <summary>Variables as a JSON object (optional)</summary>
        public object Variables { get; set; }
    }
}
