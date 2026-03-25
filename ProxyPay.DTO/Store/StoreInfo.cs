using System;
using System.Text.Json.Serialization;
using ProxyPay.DTO.Billing;

namespace ProxyPay.DTO.Store
{
    public class StoreInfo
    {
        [JsonPropertyName("storeId")]
        public long StoreId { get; set; }
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("billingStrategy")]
        public BillingStrategyEnum BillingStrategy { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
