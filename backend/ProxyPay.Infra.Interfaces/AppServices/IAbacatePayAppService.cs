using ProxyPay.DTO.AbacatePay;
using System.Threading.Tasks;

namespace ProxyPay.Infra.Interfaces.AppServices
{
    public interface IAbacatePayAppService
    {
        Task<AbacatePayResponse<BillingInfo>> CreateBillingAsync(BillingCreateRequest request, string apiKey);
        Task<AbacatePayResponse<PixQrCodeInfo>> CreatePixQrCodeAsync(PixQrCodeCreateRequest request, string apiKey);
        Task<AbacatePayResponse<PixQrCodeStatusInfo>> CheckStatusAsync(string id, string apiKey);
        Task<AbacatePayResponse<PixQrCodeInfo>> SimulatePaymentAsync(string id, string apiKey);
    }
}
