using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ProxyPay.DTO.Customer;

namespace ProxyPay.DTO.Billing
{
    public class BillingInsertInfo
    {
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }
        [JsonPropertyName("frequency")]
        public BillingFrequencyEnum Frequency { get; set; }
        [JsonPropertyName("billingStrategy")]
        public BillingStrategyEnum BillingStrategy { get; set; }
        [JsonPropertyName("billingStartDate")]
        public DateTime BillingStartDate { get; set; }
        [JsonPropertyName("customer")]
        public CustomerInsertInfo Customer { get; set; }
        [JsonPropertyName("items")]
        public IList<BillingItemInfo> Items { get; set; }
    }
}
