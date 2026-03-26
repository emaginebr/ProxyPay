using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ProxyPay.DTO.Customer;

namespace ProxyPay.DTO.Billing
{
    public class BillingRequest
    {
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }
        [JsonPropertyName("frequency")]
        public BillingFrequencyEnum Frequency { get; set; }
        [JsonPropertyName("paymentMethod")]
        public PaymentMethodEnum PaymentMethod { get; set; }
        [JsonPropertyName("billingStartDate")]
        public DateTime BillingStartDate { get; set; }
        [JsonPropertyName("completionUrl")]
        public string CompletionUrl { get; set; }
        [JsonPropertyName("returnUrl")]
        public string ReturnUrl { get; set; }
        [JsonPropertyName("customer")]
        public CustomerInsertInfo Customer { get; set; }
        [JsonPropertyName("items")]
        public IList<BillingItemInfo> Items { get; set; }
    }
}
