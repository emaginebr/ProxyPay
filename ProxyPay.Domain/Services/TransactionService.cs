using AutoMapper;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO.Transaction;
using System;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository<TransactionModel> _transactionRepository;
        private readonly IMapper _mapper;

        public TransactionService(
            ITransactionRepository<TransactionModel> transactionRepository,
            IMapper mapper
        )
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<TransactionModel> GetByIdAsync(long transactionId)
        {
            return await _transactionRepository.GetByIdAsync(transactionId);
        }

        public async Task<TransactionModel> InsertAsync(TransactionInsertInfo transaction, long storeId)
        {
            if (string.IsNullOrEmpty(transaction.Description))
                throw new Exception("Description is required");

            if (transaction.Amount <= 0)
                throw new Exception("Amount must be greater than zero");

            var currentBalance = await _transactionRepository.GetBalanceByStoreAsync(storeId);

            var newBalance = transaction.Type == TransactionTypeEnum.Credit
                ? currentBalance + transaction.Amount
                : currentBalance - transaction.Amount;

            var model = _mapper.Map<TransactionModel>(transaction);
            model.StoreId = storeId;
            model.Balance = newBalance;
            model.CreatedAt = DateTime.Now;

            return await _transactionRepository.InsertAsync(model);
        }
    }
}
