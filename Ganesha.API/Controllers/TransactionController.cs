using Ganesha.Domain.Interfaces;
using Ganesha.Domain.Mappers;
using Ganesha.DTO.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ganesha.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly ITransactionService _transactionService;

        public TransactionController(
            IUserClient userClient,
            ITransactionService transactionService
        )
        {
            _userClient = userClient;
            _transactionService = transactionService;
        }

        [Authorize]
        [HttpPost("insert")]
        public async Task<ActionResult<TransactionInfo>> Insert([FromBody] TransactionInsertInfo transaction)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var newTransaction = await _transactionService.InsertAsync(transaction, userSession.UserId);
            return Ok(TransactionMapper.ToInfo(newTransaction));
        }

        [Authorize]
        [HttpGet("getById/{transactionId}")]
        public async Task<ActionResult<TransactionInfo>> GetById(long transactionId)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var transaction = await _transactionService.GetByIdAsync(transactionId, userSession.UserId);
            if (transaction == null)
                return NotFound("Transaction not found");

            return Ok(TransactionMapper.ToInfo(transaction));
        }

        [Authorize]
        [HttpGet("balance")]
        public async Task<ActionResult<BalanceInfo>> Balance()
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            return Ok(await _transactionService.GetBalanceAsync(userSession.UserId));
        }
    }
}
