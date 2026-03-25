using System;

namespace ProxyPay.Domain.Models
{
    public class BillingItemModel
    {
        public long BillingItemId { get; set; }
        public long BillingId { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double Discount { get; set; }
        public DateTime CreatedAt { get; set; }

        public double GetTotal()
        {
            return (UnitPrice * Quantity) - Discount;
        }

        public void Update(string description, int quantity, double unitPrice, double discount)
        {
            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Discount = discount;
        }
    }
}
