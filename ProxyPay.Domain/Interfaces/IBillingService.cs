using ProxyPay.Domain.Models;
using ProxyPay.DTO.Billing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface IBillingService
    {
        Task<BillingModel> GetByIdAsync(long billingId);
        Task<BillingInfo> GetBillingInfoAsync(BillingModel model);
        Task<IList<BillingInfo>> ListByStoreAsync(long storeId);
        Task<BillingModel> InsertAsync(BillingInsertInfo billing, long storeId);
        Task DeleteAsync(long billingId);
    }
}
