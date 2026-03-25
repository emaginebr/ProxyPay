using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO.Billing;
using ProxyPay.DTO.Invoice;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ProxyPay.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        private readonly IBillingService _billingService;
        private readonly IInvoiceService _invoiceService;

        public PaymentController(
            IStoreService storeService,
            ICustomerService customerService,
            IBillingService billingService,
            IInvoiceService invoiceService
        )
        {
            _storeService = storeService;
            _customerService = customerService;
            _billingService = billingService;
            _invoiceService = invoiceService;
        }

        [HttpPost("billing")]
        public async Task<ActionResult<BillingInfo>> CreateBilling([FromBody] BillingInsertInfo billing)
        {
            try
            {
                var store = await _storeService.GetByClientIdAsync(billing.ClientId);

                var result = await _billingService.InsertAsync(billing, store.StoreId);
                return Ok(await _billingService.GetBillingInfoAsync(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("invoice")]
        public async Task<ActionResult<InvoiceInfo>> CreateInvoice([FromBody] InvoiceInsertInfo invoice)
        {
            try
            {
                var store = await _storeService.GetByClientIdAsync(invoice.ClientId);

                if (invoice.Customer == null)
                    return BadRequest("Customer is required");

                if (string.IsNullOrWhiteSpace(invoice.Customer.Email))
                    return BadRequest("Customer email is required");

                var customerId = await _customerService.UpsertAsync(invoice.Customer, store.StoreId);

                var result = await _invoiceService.InsertAsync(invoice, store.StoreId, customerId);
                return Ok(await _invoiceService.GetInvoiceInfoAsync(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
