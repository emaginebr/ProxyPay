using Ganesha.DTO.Transaction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ganesha.ACL.Interfaces
{
    public interface ITransactionClient
    {
        Task<TransactionInfo> GetByIdAsync(long transactionId);
        Task<IList<TransactionInfo>> ListAsync();
        Task<TransactionInfo> InsertAsync(TransactionInsertInfo transaction);
        Task<BalanceInfo> GetBalanceAsync();
    }
}
