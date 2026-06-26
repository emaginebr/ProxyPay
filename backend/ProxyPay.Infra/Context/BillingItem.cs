using System;

namespace ProxyPay.Infra.Context;

public partial class BillingItem
{
    public long BillingItemId { get; set; }

    public long BillingId { get; set; }

    public string Description { get; set; }

    public int Quantity { get; set; }

    public double UnitPrice { get; set; }

    public double Discount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Billing Billing { get; set; }
}
