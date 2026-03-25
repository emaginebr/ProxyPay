using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ProxyPay.DTO.Customer;

namespace ProxyPay.DTO.Invoice
{
    public class InvoiceInsertInfo
    {
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }
        [JsonPropertyName("customer")]
        public CustomerInsertInfo Customer { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        [JsonPropertyName("discount")]
        public double Discount { get; set; }
        [JsonPropertyName("dueDate")]
        public DateTime DueDate { get; set; }
        [JsonPropertyName("items")]
        public IList<InvoiceItemInsertInfo> Items { get; set; }
    }
}
