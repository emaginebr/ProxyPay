using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProxyPay.DTO.AbacatePay;
using ProxyPay.DTO.Settings;
using ProxyPay.Infra.Interfaces.AppServices;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProxyPay.Infra.AppServices
{
    public class AbacatePayAppService : IAbacatePayAppService
    {
        private readonly AbacatePaySetting _settings;
        private readonly ILogger<AbacatePayAppService> _logger;

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public AbacatePayAppService(IOptions<AbacatePaySetting> settings, ILogger<AbacatePayAppService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            return client;
        }

        private async Task<T> PostAsync<T>(string endpoint, object request)
        {
            using var client = CreateClient();
            var requestJson = JsonConvert.SerializeObject(request, _jsonSettings);
            _logger.LogInformation("AbacatePay POST {Endpoint} | Request: {Request}", endpoint, requestJson);

            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_settings.ApiUrl}{endpoint}", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("AbacatePay POST {Endpoint} | Status: {Status} | Response: {Response}",
                endpoint, (int)response.StatusCode, responseJson);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"AbacatePay error ({(int)response.StatusCode}): {responseJson}");

            return JsonConvert.DeserializeObject<T>(responseJson);
        }

        private async Task<T> GetAsync<T>(string endpoint)
        {
            using var client = CreateClient();
            _logger.LogInformation("AbacatePay GET {Endpoint}", endpoint);

            var response = await client.GetAsync($"{_settings.ApiUrl}{endpoint}");
            var responseJson = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("AbacatePay GET {Endpoint} | Status: {Status} | Response: {Response}",
                endpoint, (int)response.StatusCode, responseJson);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"AbacatePay error ({(int)response.StatusCode}): {responseJson}");

            return JsonConvert.DeserializeObject<T>(responseJson);
        }

        public async Task<AbacatePayResponse<BillingInfo>> CreateBillingAsync(BillingCreateRequest request)
        {
            return await PostAsync<AbacatePayResponse<BillingInfo>>("/v1/billing/create", request);
        }

        public async Task<AbacatePayResponse<PixQrCodeInfo>> CreatePixQrCodeAsync(PixQrCodeCreateRequest request)
        {
            return await PostAsync<AbacatePayResponse<PixQrCodeInfo>>("/v1/pixQrCode/create", request);
        }

        public async Task<AbacatePayResponse<PixQrCodeStatusInfo>> CheckStatusAsync(string id)
        {
            return await GetAsync<AbacatePayResponse<PixQrCodeStatusInfo>>($"/v1/pixQrCode/check?id={id}");
        }

        public async Task<AbacatePayResponse<PixQrCodeInfo>> SimulatePaymentAsync(string id)
        {
            return await PostAsync<AbacatePayResponse<PixQrCodeInfo>>($"/v1/pixQrCode/simulate-payment?id={id}", new { });
        }
    }
}
