using ProxyPay.DTO.Billing;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public CustomerModel Customer { get; set; }
        public IList<BillingItemModel> Items { get; set; } = new List<BillingItemModel>();

        public void AddItem(string description, int quantity, double unitPrice, double discount)
        {
            Items.Add(new BillingItemModel
            {
                Description = description,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Discount = discount,
                CreatedAt = DateTime.Now
            });
        }

        public void RemoveItem(long billingItemId)
        {
            var item = Items.FirstOrDefault(i => i.BillingItemId == billingItemId);
            if (item != null)
                Items.Remove(item);
        }

        public double GetTotal()
        {
            return Items.Sum(i => i.GetTotal());
        }

        public void Activate()
        {
            Status = BillingStatusEnum.Active;
        }

        public void Cancel()
        {
            Status = BillingStatusEnum.Cancelled;
        }

        public void UpdateFrequency(BillingFrequencyEnum frequency)
        {
            Frequency = frequency;
        }

        public void UpdateStrategy(BillingStrategyEnum strategy)
        {
            BillingStrategy = strategy;
        }
    }
}
