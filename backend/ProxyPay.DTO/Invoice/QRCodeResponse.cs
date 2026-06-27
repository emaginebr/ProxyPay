using System;
using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Invoice
{
    public class QRCodeResponse
    {
        [JsonPropertyName("invoiceId")]
        public long InvoiceId { get; set; }
        [JsonPropertyName("invoiceNumber")]
        public string InvoiceNumber { get; set; }
        [JsonPropertyName("brCode")]
        public string BrCode { get; set; }
        [JsonPropertyName("brCodeBase64")]
        public string BrCodeBase64 { get; set; }
        [JsonPropertyName("expiredAt")]
        public DateTime? ExpiredAt { get; set; }
    }
}
