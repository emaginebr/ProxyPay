using System;

namespace ProxyPay.Infra.Context;

public partial class Billing
{
    public long BillingId { get; set; }

    public long? StoreId { get; set; }

    public long? CustomerId { get; set; }

    public int Frequency { get; set; }

    public int BillingStrategy { get; set; }

    public DateTime BillingStartDate { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Store Store { get; set; }

    public virtual Customer Customer { get; set; }
}
