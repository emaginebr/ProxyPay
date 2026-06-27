using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProxyPay.DTO.GraphQL;

namespace ProxyPay.API.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/graphql-docs")]
    public class GraphQLController : ControllerBase
    {
        /// <summary>
        /// GraphQL endpoint (POST /graphql).
        /// Requires authentication. Access the interactive playground at /graphql.
        /// Supports queries: myStores, myInvoices, myInvoiceByNumber, myTransactions, myBalance, myCustomers.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(GraphQLResponse), 200)]
        public IActionResult GraphQL([FromBody] GraphQLRequest request)
        {
            return Ok(new { message = "Use POST /graphql directly. This endpoint exists for Swagger documentation only." });
        }
    }
}
