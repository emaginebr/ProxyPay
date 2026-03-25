using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ProxyPay.DTO.Customer;

namespace ProxyPay.DTO.Billing
{
    public class BillingInfo
    {
        [JsonPropertyName("billingId")]
        public long BillingId { get; set; }
        [JsonPropertyName("storeId")]
        public long? StoreId { get; set; }
        [JsonPropertyName("customerId")]
        public long? CustomerId { get; set; }
        [JsonPropertyName("frequency")]
        public BillingFrequencyEnum Frequency { get; set; }
        [JsonPropertyName("billingStartDate")]
        public DateTime BillingStartDate { get; set; }
        [JsonPropertyName("status")]
        public BillingStatusEnum Status { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("customer")]
        public CustomerInfo Customer { get; set; }
        [JsonPropertyName("items")]
        public IList<BillingItemInfo> Items { get; set; }
    }
}
