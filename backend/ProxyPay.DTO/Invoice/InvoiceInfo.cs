using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Invoice
{
    public class InvoiceInfo
    {
        [JsonPropertyName("invoiceId")]
        public long InvoiceId { get; set; }
        [JsonPropertyName("invoiceNumber")]
        public string InvoiceNumber { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        [JsonPropertyName("status")]
        public InvoiceStatusEnum Status { get; set; }
        [JsonPropertyName("paymentMethod")]
        public PaymentMethodEnum PaymentMethod { get; set; }
        [JsonPropertyName("discount")]
        public double Discount { get; set; }
        [JsonPropertyName("dueDate")]
        public DateTime DueDate { get; set; }
        [JsonPropertyName("externalCode")]
        public string ExternalCode { get; set; }
        [JsonPropertyName("expiresAt")]
        public DateTime? ExpiresAt { get; set; }
        [JsonPropertyName("paidAt")]
        public DateTime? PaidAt { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("items")]
        public IList<InvoiceItemInfo> Items { get; set; }
    }
}
