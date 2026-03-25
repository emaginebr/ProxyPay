using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyPay.Infra.Interfaces.Repository
{
    public interface ITransactionRepository<TModel> where TModel : class
    {
        Task<TModel> GetByIdAsync(long id);
        Task<IEnumerable<TModel>> ListByStoreAsync(long storeId);
        Task<TModel> InsertAsync(TModel model);
        Task<double> GetBalanceByStoreAsync(long storeId);
        Task<double> GetTotalCreditsByStoreAsync(long storeId);
        Task<double> GetTotalDebitsByStoreAsync(long storeId);
        Task<int> GetCountByStoreAsync(long storeId);
    }
}
