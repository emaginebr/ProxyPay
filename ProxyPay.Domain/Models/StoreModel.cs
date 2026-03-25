using System;

namespace ProxyPay.Domain.Models
{
    public class StoreModel
    {
        public long StoreId { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string AbacatePayApiKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
