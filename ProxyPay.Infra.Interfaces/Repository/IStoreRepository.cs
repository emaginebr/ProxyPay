using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyPay.Infra.Interfaces.Repository
{
    public interface IStoreRepository<TModel> where TModel : class
    {
        Task<TModel> GetByIdAsync(long id);
        Task<IEnumerable<TModel>> ListByUserAsync(long userId);
        Task<TModel> InsertAsync(TModel model);
        Task<TModel> UpdateAsync(TModel model);
        Task DeleteAsync(long id);
    }
}
