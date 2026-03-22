using Ganesha.Domain.Models;
using Ganesha.DTO.Transaction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ganesha.Domain.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionModel> GetByIdAsync(long transactionId, long userId);
        Task<IList<TransactionInfo>> ListByUserAsync(long userId);
        Task<TransactionModel> InsertAsync(TransactionInsertInfo transaction, long userId);
        Task<BalanceInfo> GetBalanceAsync(long userId);
    }
}
