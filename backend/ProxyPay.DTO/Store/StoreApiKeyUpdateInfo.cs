using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Store
{
    public class StoreApiKeyUpdateInfo
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }
    }
}
