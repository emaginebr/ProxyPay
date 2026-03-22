using System;

namespace Ganesha.Domain.Models
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
    }
}
