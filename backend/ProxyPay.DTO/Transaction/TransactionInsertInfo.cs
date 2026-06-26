using System.Text.Json.Serialization;

namespace ProxyPay.DTO.Transaction
{
    public class TransactionInsertInfo
    {
        [JsonPropertyName("invoiceId")]
        public long? InvoiceId { get; set; }
        [JsonPropertyName("type")]
        public TransactionTypeEnum Type { get; set; }
        [JsonPropertyName("category")]
        public TransactionCategoryEnum Category { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("amount")]
        public double Amount { get; set; }
    }
}
