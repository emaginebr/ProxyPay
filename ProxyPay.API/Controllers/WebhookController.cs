using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO.AbacatePay;
using ProxyPay.DTO.Settings;
using System;
using System.Threading.Tasks;

namespace ProxyPay.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly AbacatePaySetting _settings;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            IInvoiceService invoiceService,
            IOptions<AbacatePaySetting> settings,
            ILogger<WebhookController> logger)
        {
            _invoiceService = invoiceService;
            _settings = settings.Value;
            _logger = logger;
        }

        [HttpPost("abacatepay")]
        public async Task<IActionResult> AbacatePayWebhook(
            [FromQuery] string secret,
            [FromBody] AbacatePayWebhookPayload payload)
        {
            _logger.LogInformation("Webhook received: event={Event}, devMode={DevMode}, dataId={DataId}",
                payload?.Event, payload?.DevMode, payload?.Data?.Id);

            if (string.IsNullOrWhiteSpace(secret) || secret != _settings.WebhookSecret)
            {
                _logger.LogWarning("Webhook rejected: invalid secret");
                return Ok();
            }

            if (payload?.Data == null || string.IsNullOrWhiteSpace(payload.Event))
            {
                _logger.LogWarning("Webhook rejected: missing payload or event");
                return Ok();
            }

            try
            {
                DateTime? updatedAt = null;
                if (DateTime.TryParse(payload.Data.UpdatedAt, out var parsed))
                    updatedAt = parsed;

                await _invoiceService.ProcessWebhookAsync(payload.Event, payload.Data.Id, updatedAt);

                _logger.LogInformation("Webhook processed successfully: event={Event}, dataId={DataId}", payload.Event, payload.Data.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing error: event={Event}, dataId={DataId}", payload.Event, payload.Data?.Id);
            }

            return Ok();
        }
    }
}
