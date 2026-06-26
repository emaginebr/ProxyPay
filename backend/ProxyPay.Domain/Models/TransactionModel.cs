using ProxyPay.DTO.Transaction;
using System;

namespace ProxyPay.Domain.Models
{
    public class TransactionModel
    {
        public long TransactionId { get; set; }
        public long? InvoiceId { get; set; }
        public long? StoreId { get; set; }
        public TransactionTypeEnum Type { get; set; }
        public TransactionCategoryEnum Category { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public double Balance { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
