using Ganesha.Infra.Interfaces.Repository;
using Ganesha.Infra.Context;
using Ganesha.Infra.Mappers;
using Ganesha.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ganesha.Infra.Repository
{
    public class TransactionRepository : ITransactionRepository<TransactionModel>
    {
        private readonly GaneshaContext _context;
        private const int TYPE_CREDIT = 1;
        private const int TYPE_DEBIT = 2;

        public TransactionRepository(GaneshaContext context)
        {
            _context = context;
        }

        public async Task<TransactionModel> InsertAsync(TransactionModel model)
        {
            var row = new Transaction();
            TransactionDbMapper.ToEntity(model, row);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.TransactionId = row.TransactionId;
            return model;
        }

        public async Task<TransactionModel> GetByIdAsync(long id)
        {
            var row = await _context.Transactions.FindAsync(id);
            if (row == null)
                return null;
            return TransactionDbMapper.ToModel(row);
        }

        public async Task<IEnumerable<TransactionModel>> ListByUserAsync(long userId)
        {
            var rows = await _context.Transactions
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return rows.Select(TransactionDbMapper.ToModel);
        }

        public async Task<double> GetBalanceByUserAsync(long userId)
        {
            var lastTransaction = await _context.Transactions
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.TransactionId)
                .FirstOrDefaultAsync();

            return lastTransaction?.Balance ?? 0;
        }

        public async Task<double> GetTotalCreditsByUserAsync(long userId)
        {
            return await _context.Transactions
                .Where(x => x.UserId == userId && x.Type == TYPE_CREDIT)
                .SumAsync(x => x.Amount);
        }

        public async Task<double> GetTotalDebitsByUserAsync(long userId)
        {
            return await _context.Transactions
                .Where(x => x.UserId == userId && x.Type == TYPE_DEBIT)
                .SumAsync(x => x.Amount);
        }

        public async Task<int> GetCountByUserAsync(long userId)
        {
            return await _context.Transactions
                .Where(x => x.UserId == userId)
                .CountAsync();
        }
    }
}
