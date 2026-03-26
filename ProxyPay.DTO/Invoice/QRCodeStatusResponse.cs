using System;
using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Invoice
{
    public class QRCodeStatusResponse
    {
        [JsonPropertyName("invoiceId")]
        public long InvoiceId { get; set; }
        [JsonPropertyName("invoiceNumber")]
        public string InvoiceNumber { get; set; }
        [JsonPropertyName("paid")]
        public bool Paid { get; set; }
        [JsonPropertyName("status")]
        public InvoiceStatusEnum Status { get; set; }
        [JsonPropertyName("statusText")]
        public string StatusText { get; set; }
        [JsonPropertyName("expiresAt")]
        public DateTime? ExpiresAt { get; set; }
    }
}
