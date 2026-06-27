using ProxyPay.DTO.AbacatePay;
using System.Threading.Tasks;

namespace ProxyPay.Infra.Interfaces.AppServices
{
    public interface IAbacatePayAppService
    {
        Task<AbacatePayResponse<BillingInfo>> CreateBillingAsync(BillingCreateRequest request);
        Task<AbacatePayResponse<PixQrCodeInfo>> CreatePixQrCodeAsync(PixQrCodeCreateRequest request);
        Task<AbacatePayResponse<PixQrCodeStatusInfo>> CheckStatusAsync(string id);
        Task<AbacatePayResponse<PixQrCodeInfo>> SimulatePaymentAsync(string id);
    }
}
