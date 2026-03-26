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
        public async Task<ActionResult<BillingResponse>> CreateBilling([FromBody] BillingRequest billing)
        {
            try
            {
                var store = await _storeService.GetByClientIdAsync(billing.ClientId);

                if (billing.Customer == null)
                    return BadRequest("Customer is required");

                if (string.IsNullOrWhiteSpace(billing.Customer.Email))
                    return BadRequest("Customer email is required");

                var customerId = await _customerService.UpsertAsync(billing.Customer, store.StoreId);

                var result = await _billingService.CreateBillingAsync(billing, store.StoreId, customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("invoice")]
        public async Task<ActionResult<InvoiceResponse>> CreateInvoice([FromBody] InvoiceRequest invoice)
        {
            try
            {
                var store = await _storeService.GetByClientIdAsync(invoice.ClientId);

                if (invoice.Customer == null)
                    return BadRequest("Customer is required");

                if (string.IsNullOrWhiteSpace(invoice.Customer.Email))
                    return BadRequest("Customer email is required");

                var customerId = await _customerService.UpsertAsync(invoice.Customer, store.StoreId);

                var result = await _invoiceService.CreateInvoiceAsync(invoice, store.StoreId, customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("qrcode")]
        public async Task<ActionResult<QRCodeResponse>> CreateQRCode([FromBody] QRCodeRequest request)
        {
            try
            {
                var store = await _storeService.GetByClientIdAsync(request.ClientId);

                if (request.Customer == null)
                    return BadRequest("Customer is required");

                if (string.IsNullOrWhiteSpace(request.Customer.Email))
                    return BadRequest("Customer email is required");

                var customerId = await _customerService.UpsertAsync(request.Customer, store.StoreId);

                var result = await _invoiceService.CreateQRCodeAsync(request, store.StoreId, customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("qrcode/status/{invoiceId}")]
        public async Task<ActionResult<QRCodeStatusResponse>> CheckQRCodeStatus(long invoiceId)
        {
            try
            {
                var result = await _invoiceService.CheckQRCodeStatusAsync(invoiceId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
