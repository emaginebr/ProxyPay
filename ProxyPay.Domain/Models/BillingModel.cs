using ProxyPay.DTO.Billing;
using System;

namespace ProxyPay.Domain.Models
{
    public class BillingModel
    {
        public long BillingId { get; set; }
        public long? StoreId { get; set; }
        public long? CustomerId { get; set; }
        public BillingFrequencyEnum Frequency { get; set; }
        public BillingStrategyEnum BillingStrategy { get; set; }
        public DateTime BillingStartDate { get; set; }
        public BillingStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
