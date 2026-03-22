using Ganesha.Domain.Models;
using Ganesha.DTO.Invoice;
using Ganesha.Infra.Context;

namespace Ganesha.Infra.Mappers
{
    public static class InvoiceDbMapper
    {
        public static InvoiceModel ToModel(Invoice row)
        {
            return new InvoiceModel
            {
                InvoiceId = row.InvoiceId,
                UserId = row.UserId,
                InvoiceNumber = row.InvoiceNumber,
                Notes = row.Notes,
                Status = (InvoiceStatusEnum)row.Status,
                SubTotal = row.SubTotal,
                Discount = row.Discount,
                Tax = row.Tax,
                Total = row.Total,
                DueDate = row.DueDate,
                PaidAt = row.PaidAt,
                CreatedAt = row.CreatedAt,
                UpdatedAt = row.UpdatedAt
            };
        }

        public static void ToEntity(InvoiceModel md, Invoice row)
        {
            row.InvoiceId = md.InvoiceId;
            row.UserId = md.UserId;
            row.InvoiceNumber = md.InvoiceNumber;
            row.Notes = md.Notes;
            row.Status = (int)md.Status;
            row.SubTotal = md.SubTotal;
            row.Discount = md.Discount;
            row.Tax = md.Tax;
            row.Total = md.Total;
            row.DueDate = md.DueDate;
            row.PaidAt = md.PaidAt;
            row.CreatedAt = md.CreatedAt;
            row.UpdatedAt = md.UpdatedAt;
        }
    }
}
