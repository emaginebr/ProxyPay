using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProxyPay.DTO.AbacatePay;
using ProxyPay.DTO.Settings;
using ProxyPay.Infra.Interfaces.AppServices;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProxyPay.Infra.AppServices
{
    public class AbacatePayAppService : IAbacatePayAppService
    {
        private readonly HttpClient _httpClient;
        private readonly AbacatePaySetting _settings;

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public AbacatePayAppService(IOptions<AbacatePaySetting> settings)
        {
            _settings = settings.Value;
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        public async Task<AbacatePayResponse<BillingInfo>> CreateBillingAsync(BillingCreateRequest request)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(request, _jsonSettings),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_settings.ApiUrl}/v1/billing/create", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AbacatePayResponse<BillingInfo>>(json);
        }

        public async Task<AbacatePayResponse<PixQrCodeInfo>> CreatePixQrCodeAsync(PixQrCodeCreateRequest request)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(request, _jsonSettings),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_settings.ApiUrl}/v1/pixQrCode/create", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AbacatePayResponse<PixQrCodeInfo>>(json);
        }

        public async Task<AbacatePayResponse<PixQrCodeStatusInfo>> CheckStatusAsync(string id)
        {
            var response = await _httpClient.GetAsync($"{_settings.ApiUrl}/v1/pixQrCode/check?id={id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AbacatePayResponse<PixQrCodeStatusInfo>>(json);
        }

        public async Task<AbacatePayResponse<PixQrCodeInfo>> SimulatePaymentAsync(string id)
        {
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_settings.ApiUrl}/v1/pixQrCode/simulate-payment?id={id}", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AbacatePayResponse<PixQrCodeInfo>>(json);
        }
    }
}
