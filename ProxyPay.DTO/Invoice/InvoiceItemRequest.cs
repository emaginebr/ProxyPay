using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Invoice
{
    public class InvoiceItemRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
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
