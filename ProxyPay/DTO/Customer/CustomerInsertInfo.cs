using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Customer
{
    public class CustomerInsertInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }
        [JsonPropertyName("cellphone")]
        public string Cellphone { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
