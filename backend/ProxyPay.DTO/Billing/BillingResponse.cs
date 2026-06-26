using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Billing
{
    public class BillingResponse
    {
        [JsonPropertyName("billingId")]
        public long BillingId { get; set; }
        [JsonPropertyName("invoiceId")]
        public long InvoiceId { get; set; }
        [JsonPropertyName("invoiceNumber")]
        public string InvoiceNumber { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
