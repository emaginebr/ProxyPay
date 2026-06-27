using System;

namespace ProxyPay.Domain.Models
{
    public class InvoiceItemModel
    {
        public long InvoiceItemId { get; set; }
        public long InvoiceId { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public DateTime CreatedAt { get; set; }

        public void CalculateTotal()
        {
            Total = (Quantity * UnitPrice) - Discount;
        }

        public void Update(string description, int quantity, double unitPrice, double discount)
        {
            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Discount = discount;
            CalculateTotal();
        }

        public void MarkCreated()
        {
            CreatedAt = DateTime.Now;
            CalculateTotal();
        }
    }
}
