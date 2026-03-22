using System;
using System.Text.Json.Serialization;

namespace Ganesha.DTO.Transaction
{
    public class TransactionInfo
    {
        [JsonPropertyName("transactionId")]
        public long TransactionId { get; set; }
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
        [JsonPropertyName("balance")]
        public double Balance { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
