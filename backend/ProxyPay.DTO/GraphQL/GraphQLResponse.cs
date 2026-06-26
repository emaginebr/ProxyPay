namespace ProxyPay.DTO.GraphQL
{
    public class GraphQLResponse
    {
        /// <summary>Query result data</summary>
        public object Data { get; set; }

        /// <summary>Errors, if any</summary>
        public object Errors { get; set; }
    }
}
