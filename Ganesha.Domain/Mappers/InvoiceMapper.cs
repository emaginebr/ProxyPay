using Ganesha.Domain.Models;
using Ganesha.DTO.Invoice;

namespace Ganesha.Domain.Mappers
{
    public static class InvoiceMapper
    {
        public static InvoiceInfo ToInfo(InvoiceModel md)
        {
            return new InvoiceInfo
            {
                InvoiceId = md.InvoiceId,
                InvoiceNumber = md.InvoiceNumber,
                Notes = md.Notes,
                Status = md.Status,
                SubTotal = md.SubTotal,
                Discount = md.Discount,
                Tax = md.Tax,
                Total = md.Total,
                DueDate = md.DueDate,
                PaidAt = md.PaidAt,
                CreatedAt = md.CreatedAt,
                UpdatedAt = md.UpdatedAt
            };
        }
    }
}
