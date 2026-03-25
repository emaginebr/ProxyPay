using AutoMapper;
using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyPay.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly ITransactionService _transactionService;
        private readonly IMapper _mapper;

        public TransactionController(
            IUserClient userClient,
            ITransactionService transactionService,
            IMapper mapper
        )
        {
            _userClient = userClient;
            _transactionService = transactionService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpPost("{storeId}/insert")]
        public async Task<ActionResult<TransactionInfo>> Insert(long storeId, [FromBody] TransactionInsertInfo transaction)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var newTransaction = await _transactionService.InsertAsync(transaction, storeId);
            return Ok(_mapper.Map<TransactionInfo>(newTransaction));
        }

        [Authorize]
        [HttpGet("getById/{transactionId}")]
        public async Task<ActionResult<TransactionInfo>> GetById(long transactionId)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction == null)
                return NotFound("Transaction not found");

            return Ok(_mapper.Map<TransactionInfo>(transaction));
        }

        [Authorize]
        [HttpGet("{storeId}/balance")]
        public async Task<ActionResult<BalanceInfo>> Balance(long storeId)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            return Ok(await _transactionService.GetBalanceAsync(storeId));
        }
    }
}
