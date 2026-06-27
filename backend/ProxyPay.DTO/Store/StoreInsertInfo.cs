using System.Text.Json.Serialization;
using ProxyPay.DTO.Billing;

namespace ProxyPay.DTO.Store
{
    public class StoreInsertInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("billingStrategy")]
        public BillingStrategyEnum BillingStrategy { get; set; }
    }
}
