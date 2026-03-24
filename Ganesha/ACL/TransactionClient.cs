using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ganesha.ACL.Core;
using Ganesha.ACL.Interfaces;
using Ganesha.DTO.Transaction;
using Ganesha.DTO.Settings;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ganesha.ACL
{
    public class TransactionClient : BaseClient, ITransactionClient
    {
        public TransactionClient(IOptions<GaneshaSetting> nsalesSetting) : base(nsalesSetting)
        {
        }

        public async Task<TransactionInfo> GetByIdAsync(long transactionId)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Transaction/getById/{transactionId}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<TransactionInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<IList<TransactionInfo>> ListAsync()
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Transaction/list");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<IList<TransactionInfo>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<TransactionInfo> InsertAsync(TransactionInsertInfo transaction)
        {
            var content = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Transaction/insert", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<TransactionInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<BalanceInfo> GetBalanceAsync()
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Transaction/balance");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<BalanceInfo>(await response.Content.ReadAsStringAsync());
        }
    }
}
