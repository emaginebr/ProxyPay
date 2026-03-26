using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ProxyPay.DTO.Customer;

namespace ProxyPay.DTO.Invoice
{
    public class InvoiceRequest
    {
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }
        [JsonPropertyName("customer")]
        public CustomerInsertInfo Customer { get; set; }
        [JsonPropertyName("paymentMethod")]
        public PaymentMethodEnum PaymentMethod { get; set; }
        [JsonPropertyName("completionUrl")]
        public string CompletionUrl { get; set; }
        [JsonPropertyName("returnUrl")]
        public string ReturnUrl { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
        [JsonPropertyName("discount")]
        public double Discount { get; set; }
        [JsonPropertyName("dueDate")]
        public DateTime DueDate { get; set; }
        [JsonPropertyName("items")]
        public IList<InvoiceItemRequest> Items { get; set; }
    }
}
