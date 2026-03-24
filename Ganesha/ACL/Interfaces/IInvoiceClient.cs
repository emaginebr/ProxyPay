using Ganesha.DTO.Invoice;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ganesha.ACL.Interfaces
{
    public interface IInvoiceClient
    {
        Task<InvoiceInfo> GetByIdAsync(long invoiceId);
        Task<IList<InvoiceInfo>> ListAsync();
        Task<InvoiceInfo> InsertAsync(InvoiceInsertInfo invoice);
        Task<InvoiceInfo> UpdateAsync(InvoiceUpdateInfo invoice);
        Task DeleteAsync(long invoiceId);
    }
}
