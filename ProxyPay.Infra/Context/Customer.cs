using System;
using System.Collections.Generic;

namespace ProxyPay.Infra.Context;

public partial class Customer
{
    public long CustomerId { get; set; }

    public long? StoreId { get; set; }

    public string Name { get; set; }

    public string DocumentId { get; set; }

    public string Cellphone { get; set; }

    public string Email { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Store Store { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();
}
