using Ganesha.Infra.Interfaces.Repository;
using Ganesha.Domain.Mappers;
using Ganesha.Domain.Models;
using Ganesha.Domain.Interfaces;
using Ganesha.DTO.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ganesha.Domain.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository<TransactionModel> _transactionRepository;

        public TransactionService(
            ITransactionRepository<TransactionModel> transactionRepository
        )
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<TransactionModel> GetByIdAsync(long transactionId, long userId)
        {
            var model = await _transactionRepository.GetByIdAsync(transactionId);
            if (model == null)
                return null;

            if (model.UserId != userId)
                throw new UnauthorizedAccessException("Access denied: transaction does not belong to this user");

            return model;
        }

        public async Task<IList<TransactionInfo>> ListByUserAsync(long userId)
        {
            var transactions = await _transactionRepository.ListByUserAsync(userId);
            return transactions.Select(TransactionMapper.ToInfo).ToList();
        }

        public async Task<TransactionModel> InsertAsync(TransactionInsertInfo transaction, long userId)
        {
            if (string.IsNullOrEmpty(transaction.Description))
                throw new Exception("Description is required");

            if (transaction.Amount <= 0)
                throw new Exception("Amount must be greater than zero");

            var currentBalance = await _transactionRepository.GetBalanceByUserAsync(userId);

            var newBalance = transaction.Type == TransactionTypeEnum.Credit
                ? currentBalance + transaction.Amount
                : currentBalance - transaction.Amount;

            var model = new TransactionModel
            {
                UserId = userId,
                InvoiceId = transaction.InvoiceId,
                Type = transaction.Type,
                Category = transaction.Category,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Balance = newBalance,
                CreatedAt = DateTime.Now
            };

            return await _transactionRepository.InsertAsync(model);
        }

        public async Task<BalanceInfo> GetBalanceAsync(long userId)
        {
            return new BalanceInfo
            {
                Balance = await _transactionRepository.GetBalanceByUserAsync(userId),
                TotalCredits = await _transactionRepository.GetTotalCreditsByUserAsync(userId),
                TotalDebits = await _transactionRepository.GetTotalDebitsByUserAsync(userId),
                TransactionCount = await _transactionRepository.GetCountByUserAsync(userId)
            };
        }
    }
}
