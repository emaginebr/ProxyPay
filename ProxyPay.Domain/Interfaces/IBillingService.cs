using ProxyPay.Domain.Models;
using ProxyPay.DTO.Billing;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface IBillingService
    {
        Task<BillingModel> GetByIdAsync(long billingId);
        Task<BillingInfo> GetBillingInfoAsync(BillingModel model);
        Task<BillingResponse> CreateBillingAsync(BillingRequest request, long storeId, long customerId);
        Task<BillingModel> InsertAsync(BillingRequest billing, long storeId);
        Task<BillingModel> UpdateAsync(long billingId, BillingRequest billing);
        Task DeleteAsync(long billingId);
    }
}
