using ProxyPay.DTO.Billing;
using System;

namespace ProxyPay.Domain.Models
{
    public class StoreModel
    {
        public long StoreId { get; set; }
        public string ClientId { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public BillingStrategyEnum BillingStrategy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public void GenerateClientId()
        {
            ClientId = Guid.NewGuid().ToString("N");
        }

        public void SetOwner(long userId)
        {
            UserId = userId;
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            Name = name;
            MarkUpdated();
        }

        public void UpdateEmail(string email)
        {
            Email = email;
            MarkUpdated();
        }

        public void UpdateBillingStrategy(BillingStrategyEnum strategy)
        {
            BillingStrategy = strategy;
            MarkUpdated();
        }

        public void ValidateOwnership(long userId)
        {
            if (UserId != userId)
                throw new UnauthorizedAccessException("Access denied: store does not belong to this user");
        }

        public void MarkCreated()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public void MarkUpdated()
        {
            UpdatedAt = DateTime.Now;
        }
    }
}
