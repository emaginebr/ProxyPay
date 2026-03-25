using AutoMapper;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Infra.Context;
using ProxyPay.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyPay.Infra.Repository
{
    public class TransactionRepository : ITransactionRepository<TransactionModel>
    {
        private readonly ProxyPayContext _context;
        private readonly IMapper _mapper;
        private const int TYPE_CREDIT = 1;
        private const int TYPE_DEBIT = 2;

        public TransactionRepository(ProxyPayContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TransactionModel> InsertAsync(TransactionModel model)
        {
            var row = _mapper.Map<Transaction>(model);
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
            return _mapper.Map<TransactionModel>(row);
        }

        public async Task<IEnumerable<TransactionModel>> ListByStoreAsync(long storeId)
        {
            var rows = await _context.Transactions
                .Where(x => x.StoreId == storeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return rows.Select(r => _mapper.Map<TransactionModel>(r));
        }

        public async Task<double> GetBalanceByStoreAsync(long storeId)
        {
            var lastTransaction = await _context.Transactions
                .Where(x => x.StoreId == storeId)
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.TransactionId)
                .FirstOrDefaultAsync();

            return lastTransaction?.Balance ?? 0;
        }

        public async Task<double> GetTotalCreditsByStoreAsync(long storeId)
        {
            return await _context.Transactions
                .Where(x => x.StoreId == storeId && x.Type == TYPE_CREDIT)
                .SumAsync(x => x.Amount);
        }

        public async Task<double> GetTotalDebitsByStoreAsync(long storeId)
        {
            return await _context.Transactions
                .Where(x => x.StoreId == storeId && x.Type == TYPE_DEBIT)
                .SumAsync(x => x.Amount);
        }

        public async Task<int> GetCountByStoreAsync(long storeId)
        {
            return await _context.Transactions
                .Where(x => x.StoreId == storeId)
                .CountAsync();
        }
    }
}
