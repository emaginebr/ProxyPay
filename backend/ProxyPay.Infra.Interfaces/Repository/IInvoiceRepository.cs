using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyPay.Infra.Interfaces.Repository
{
    public interface IInvoiceRepository<TModel> where TModel : class
    {
        Task<TModel> GetByIdAsync(long id);
        Task<TModel> GetByNumberAsync(string invoiceNumber);
        Task<TModel> GetByExternalCodeAsync(string externalCode);
        Task<IEnumerable<TModel>> ListByStoreAsync(long storeId);
        Task<TModel> InsertAsync(TModel model);
        Task<TModel> UpdateAsync(TModel model);
        Task DeleteAsync(long id);
        Task<string> GenerateInvoiceNumberAsync(long storeId);
    }
}
