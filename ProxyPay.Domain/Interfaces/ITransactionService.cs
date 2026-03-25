using ProxyPay.Domain.Models;
using ProxyPay.DTO.Transaction;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionModel> GetByIdAsync(long transactionId);
        Task<TransactionModel> InsertAsync(TransactionInsertInfo transaction, long storeId);
    }
}
