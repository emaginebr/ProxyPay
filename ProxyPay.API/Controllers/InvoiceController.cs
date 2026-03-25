using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO.Invoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyPay.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(
            IUserClient userClient,
            IInvoiceService invoiceService
        )
        {
            _userClient = userClient;
            _invoiceService = invoiceService;
        }

        [Authorize]
        [HttpPost("{storeId}/insert")]
        public async Task<ActionResult<InvoiceInfo>> Insert(long storeId, [FromBody] InvoiceInsertInfo invoice)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var newInvoice = await _invoiceService.InsertAsync(invoice, storeId);
            return Ok(await _invoiceService.GetInvoiceInfoAsync(newInvoice));
        }

        [Authorize]
        [HttpPost("update")]
        public async Task<ActionResult<InvoiceInfo>> Update([FromBody] InvoiceUpdateInfo invoice)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var updated = await _invoiceService.UpdateAsync(invoice);
            return Ok(await _invoiceService.GetInvoiceInfoAsync(updated));
        }

        [Authorize]
        [HttpGet("{storeId}/list")]
        public async Task<ActionResult<IList<InvoiceInfo>>> List(long storeId)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            return Ok(await _invoiceService.ListByStoreAsync(storeId));
        }

        [Authorize]
        [HttpGet("getById/{invoiceId}")]
        public async Task<ActionResult<InvoiceInfo>> GetById(long invoiceId)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var invoice = await _invoiceService.GetByIdAsync(invoiceId);
            if (invoice == null)
                return NotFound("Invoice not found");

            return Ok(await _invoiceService.GetInvoiceInfoAsync(invoice));
        }

        [Authorize]
        [HttpDelete("delete/{invoiceId}")]
        public async Task<ActionResult> Delete(long invoiceId)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            await _invoiceService.DeleteAsync(invoiceId);
            return Ok();
        }
    }
}
