using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ganesha.API.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/graphql-docs")]
    public class GraphQLController : ControllerBase
    {
        /// <summary>
        /// GraphQL endpoint (POST /graphql).
        /// Requires authentication. Access the interactive playground at /graphql.
        /// Supports queries: myInvoices, myTransactions.
        /// </summary>
        [HttpPost("graphql")]
        [Authorize]
        [ProducesResponseType(typeof(GraphQLResponse), 200)]
        public IActionResult GraphQL([FromBody] GraphQLRequest request)
        {
            return Ok(new { message = "Use POST /graphql directly. This endpoint exists for Swagger documentation only." });
        }
    }

    public class GraphQLRequest
    {
        /// <summary>GraphQL query string</summary>
        /// <example>{ myInvoices { items { invoiceId invoiceNumber total } } }</example>
        public string Query { get; set; }

        /// <summary>Operation name (optional, for multi-operation documents)</summary>
        public string OperationName { get; set; }

        /// <summary>Variables as a JSON object (optional)</summary>
        public object Variables { get; set; }
    }

    public class GraphQLResponse
    {
        /// <summary>Query result data</summary>
        public object Data { get; set; }

        /// <summary>Errors, if any</summary>
        public object Errors { get; set; }
    }
}
