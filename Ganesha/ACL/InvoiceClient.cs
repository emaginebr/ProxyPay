using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ganesha.ACL.Core;
using Ganesha.ACL.Interfaces;
using Ganesha.DTO.Invoice;
using Ganesha.DTO.Settings;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ganesha.ACL
{
    public class InvoiceClient : BaseClient, IInvoiceClient
    {
        public InvoiceClient(IOptions<GaneshaSetting> nsalesSetting) : base(nsalesSetting)
        {
        }

        public async Task<InvoiceInfo> GetByIdAsync(long invoiceId)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Invoice/getById/{invoiceId}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<InvoiceInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<IList<InvoiceInfo>> ListAsync()
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Invoice/list");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<IList<InvoiceInfo>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<InvoiceInfo> InsertAsync(InvoiceInsertInfo invoice)
        {
            var content = new StringContent(JsonConvert.SerializeObject(invoice), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Invoice/insert", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<InvoiceInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<InvoiceInfo> UpdateAsync(InvoiceUpdateInfo invoice)
        {
            var content = new StringContent(JsonConvert.SerializeObject(invoice), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Invoice/update", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<InvoiceInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAsync(long invoiceId)
        {
            var response = await _httpClient.DeleteAsync($"{_nsalesSetting.Value.ApiUrl}/Invoice/delete/{invoiceId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
