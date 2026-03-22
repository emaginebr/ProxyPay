using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ganesha.Infra.Interfaces.Repository
{
    public interface ITransactionRepository<TModel> where TModel : class
    {
        Task<TModel> GetByIdAsync(long id);
        Task<IEnumerable<TModel>> ListByUserAsync(long userId);
        Task<TModel> InsertAsync(TModel model);
        Task<double> GetBalanceByUserAsync(long userId);
        Task<double> GetTotalCreditsByUserAsync(long userId);
        Task<double> GetTotalDebitsByUserAsync(long userId);
        Task<int> GetCountByUserAsync(long userId);
    }
}
