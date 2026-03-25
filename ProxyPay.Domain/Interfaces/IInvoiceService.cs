using ProxyPay.Domain.Models;
using ProxyPay.DTO.Invoice;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceModel> GetByIdAsync(long invoiceId);
        Task<InvoiceInfo> GetInvoiceInfoAsync(InvoiceModel model);
        Task<IList<InvoiceInfo>> ListByStoreAsync(long storeId);
        Task<InvoiceModel> InsertAsync(InvoiceInsertInfo invoice, long storeId);
        Task<InvoiceModel> UpdateAsync(InvoiceUpdateInfo invoice);
        Task DeleteAsync(long invoiceId);
    }
}
