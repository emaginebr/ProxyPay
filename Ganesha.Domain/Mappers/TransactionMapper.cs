using Ganesha.Domain.Models;
using Ganesha.DTO.Transaction;

namespace Ganesha.Domain.Mappers
{
    public static class TransactionMapper
    {
        public static TransactionInfo ToInfo(TransactionModel md)
        {
            return new TransactionInfo
            {
                TransactionId = md.TransactionId,
                InvoiceId = md.InvoiceId,
                Type = md.Type,
                Category = md.Category,
                Description = md.Description,
                Amount = md.Amount,
                Balance = md.Balance,
                CreatedAt = md.CreatedAt
            };
        }
    }
}
