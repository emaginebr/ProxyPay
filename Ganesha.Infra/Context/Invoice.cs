using System;
using System.Collections.Generic;

namespace Ganesha.Infra.Context;

public partial class Invoice
{
    public long InvoiceId { get; set; }

    public long UserId { get; set; }

    public string InvoiceNumber { get; set; }

    public string Notes { get; set; }

    public int Status { get; set; }

    public double SubTotal { get; set; }

    public double Discount { get; set; }

    public double Tax { get; set; }

    public double Total { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
