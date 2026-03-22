using System;
using System.Text.Json.Serialization;

namespace Ganesha.DTO.Invoice
{
    public class InvoiceItemInfo
    {
        [JsonPropertyName("invoiceItemId")]
        public long InvoiceItemId { get; set; }
        [JsonPropertyName("invoiceId")]
        public long InvoiceId { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("unitPrice")]
        public double UnitPrice { get; set; }
        [JsonPropertyName("discount")]
        public double Discount { get; set; }
        [JsonPropertyName("total")]
        public double Total { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
