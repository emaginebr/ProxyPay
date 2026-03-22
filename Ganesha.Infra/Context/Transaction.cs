using System;

namespace Ganesha.Infra.Context;

public partial class Transaction
{
    public long TransactionId { get; set; }

    public long UserId { get; set; }

    public long? InvoiceId { get; set; }

    public int Type { get; set; }

    public int Category { get; set; }

    public string Description { get; set; }

    public double Amount { get; set; }

    public double Balance { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Invoice Invoice { get; set; }
}
