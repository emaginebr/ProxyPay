using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Transaction
{
    public class BalanceInfo
    {
        [JsonPropertyName("balance")]
        public double Balance { get; set; }
        [JsonPropertyName("totalCredits")]
        public double TotalCredits { get; set; }
        [JsonPropertyName("totalDebits")]
        public double TotalDebits { get; set; }
        [JsonPropertyName("transactionCount")]
        public int TransactionCount { get; set; }
    }
}
