using System;
using System.Collections.Generic;

namespace ProxyPay.Infra.Context;

public partial class Invoice
{
    public long InvoiceId { get; set; }

    public long? CustomerId { get; set; }

    public long? StoreId { get; set; }

    public string InvoiceNumber { get; set; }

    public string Notes { get; set; }

    public int Status { get; set; }

    public int PaymentMethod { get; set; }

    public double Discount { get; set; }

    public DateTime DueDate { get; set; }

    public string ExternalCode { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Store Store { get; set; }

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
