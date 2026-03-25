using ProxyPay.Domain.Models;
using ProxyPay.DTO.Transaction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionModel> GetByIdAsync(long transactionId);
        Task<IList<TransactionInfo>> ListByStoreAsync(long storeId);
        Task<TransactionModel> InsertAsync(TransactionInsertInfo transaction, long storeId);
        Task<BalanceInfo> GetBalanceAsync(long storeId);
    }
}
