using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Store
{
    public class StoreInsertInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("abacatePayApiKey")]
        public string AbacatePayApiKey { get; set; }
    }
}
