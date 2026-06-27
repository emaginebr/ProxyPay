using System;

namespace ProxyPay.Infra.Context;

public partial class InvoiceItem
{
    public long InvoiceItemId { get; set; }

    public long InvoiceId { get; set; }

    public string Description { get; set; }

    public int Quantity { get; set; }

    public double UnitPrice { get; set; }

    public double Discount { get; set; }

    public double Total { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Invoice Invoice { get; set; }
}
