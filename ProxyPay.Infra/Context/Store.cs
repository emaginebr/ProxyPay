using System;
using System.Collections.Generic;

namespace ProxyPay.Infra.Context;

public partial class Store
{
    public long StoreId { get; set; }

    public string ClientId { get; set; }

    public long UserId { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string AbacatePayApiKey { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();
}
