using ProxyPay.Domain.Models;
using ProxyPay.DTO.Invoice;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceModel> GetByIdAsync(long invoiceId);
        Task<InvoiceInfo> GetInvoiceInfoAsync(InvoiceModel model);
        Task<InvoiceModel> InsertAsync(InvoiceInsertInfo invoice, long storeId);
        Task<InvoiceModel> InsertAsync(InvoiceInsertInfo invoice, long storeId, long customerId);
        Task<InvoiceModel> UpdateAsync(InvoiceUpdateInfo invoice);
        Task DeleteAsync(long invoiceId);
    }
}
