using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Invoice
{
    public class InvoiceResponse
    {
        [JsonPropertyName("invoiceId")]
        public long InvoiceId { get; set; }
        [JsonPropertyName("invoiceNumber")]
        public string InvoiceNumber { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
