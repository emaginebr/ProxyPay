using ProxyPay.Domain.Models;
using ProxyPay.DTO.Store;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface IStoreService
    {
        Task<StoreModel> GetByIdAsync(long storeId, long userId);
        Task<StoreModel> GetByClientIdAsync(string clientId);
        Task<StoreModel> InsertAsync(StoreInsertInfo store, long userId);
        Task<StoreModel> UpdateAsync(StoreUpdateInfo store, long userId);
        Task DeleteAsync(long storeId, long userId);
    }
}
