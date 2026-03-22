using Ganesha.Domain.Models;
using Ganesha.DTO.Invoice;

namespace Ganesha.Domain.Mappers
{
    public static class InvoiceItemMapper
    {
        public static InvoiceItemInfo ToInfo(InvoiceItemModel md)
        {
            return new InvoiceItemInfo
            {
                InvoiceItemId = md.InvoiceItemId,
                InvoiceId = md.InvoiceId,
                Description = md.Description,
                Quantity = md.Quantity,
                UnitPrice = md.UnitPrice,
                Discount = md.Discount,
                Total = md.Total,
                CreatedAt = md.CreatedAt
            };
        }
    }
}
