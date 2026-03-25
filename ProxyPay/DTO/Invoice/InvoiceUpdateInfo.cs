using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Invoice
{
    public class InvoiceUpdateInfo
    {
        [JsonPropertyName("invoiceId")]
        public long InvoiceId { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        [JsonPropertyName("status")]
        public InvoiceStatusEnum Status { get; set; }
        [JsonPropertyName("discount")]
        public double Discount { get; set; }
        [JsonPropertyName("dueDate")]
        public DateTime DueDate { get; set; }
        [JsonPropertyName("items")]
        public IList<InvoiceItemInsertInfo> Items { get; set; }
    }
}
