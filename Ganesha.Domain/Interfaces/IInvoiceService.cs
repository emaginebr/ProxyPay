using Ganesha.Domain.Models;
using Ganesha.DTO.Invoice;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ganesha.Domain.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceModel> GetByIdAsync(long invoiceId, long userId);
        Task<InvoiceInfo> GetInvoiceInfoAsync(InvoiceModel model);
        Task<IList<InvoiceInfo>> ListByUserAsync(long userId);
        Task<InvoiceModel> InsertAsync(InvoiceInsertInfo invoice, long userId);
        Task<InvoiceModel> UpdateAsync(InvoiceUpdateInfo invoice, long userId);
        Task DeleteAsync(long invoiceId, long userId);
    }
}
