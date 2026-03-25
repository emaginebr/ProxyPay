using System;
using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Customer
{
    public class CustomerInfo
    {
        [JsonPropertyName("customerId")]
        public long CustomerId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }
        [JsonPropertyName("cellphone")]
        public string Cellphone { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
