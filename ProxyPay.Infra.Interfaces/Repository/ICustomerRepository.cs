using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyPay.Infra.Interfaces.Repository
{
    public interface ICustomerRepository<TModel> where TModel : class
    {
        Task<TModel> GetByIdAsync(long id);
        Task<TModel> GetByEmailAndStoreAsync(string email, long storeId);
        Task<IEnumerable<TModel>> ListByStoreAsync(long storeId);
        Task<TModel> InsertAsync(TModel model);
        Task<TModel> UpdateAsync(TModel model);
        Task DeleteAsync(long id);
    }
}
