using System;

namespace ProxyPay.Domain.Models
{
    public class CustomerModel
    {
        public long CustomerId { get; set; }
        public long? StoreId { get; set; }
        public string Name { get; set; }
        public string DocumentId { get; set; }
        public string Cellphone { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
