using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Billing
{
    public class BillingItemInfo
    {
        [JsonPropertyName("billingItemId")]
        public long BillingItemId { get; set; }
        [JsonPropertyName("billingId")]
        public long BillingId { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("unitPrice")]
        public double UnitPrice { get; set; }
        [JsonPropertyName("discount")]
        public double Discount { get; set; }
    }
}
