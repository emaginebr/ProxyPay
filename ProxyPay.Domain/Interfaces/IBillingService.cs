using ProxyPay.Domain.Models;
using ProxyPay.DTO.Billing;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface IBillingService
    {
        Task<BillingModel> GetByIdAsync(long billingId);
        Task<BillingInfo> GetBillingInfoAsync(BillingModel model);
        Task<BillingModel> InsertAsync(BillingInsertInfo billing, long storeId);
        Task<BillingModel> UpdateAsync(long billingId, BillingInsertInfo billing);
        Task DeleteAsync(long billingId);
    }
}
