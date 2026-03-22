using Ganesha.Domain.Models;
using Ganesha.DTO.Transaction;
using Ganesha.Infra.Context;

namespace Ganesha.Infra.Mappers
{
    public static class TransactionDbMapper
    {
        public static TransactionModel ToModel(Transaction row)
        {
            return new TransactionModel
            {
                TransactionId = row.TransactionId,
                UserId = row.UserId,
                InvoiceId = row.InvoiceId,
                Type = (TransactionTypeEnum)row.Type,
                Category = (TransactionCategoryEnum)row.Category,
                Description = row.Description,
                Amount = row.Amount,
                Balance = row.Balance,
                CreatedAt = row.CreatedAt
            };
        }

        public static void ToEntity(TransactionModel md, Transaction row)
        {
            row.TransactionId = md.TransactionId;
            row.UserId = md.UserId;
            row.InvoiceId = md.InvoiceId;
            row.Type = (int)md.Type;
            row.Category = (int)md.Category;
            row.Description = md.Description;
            row.Amount = md.Amount;
            row.Balance = md.Balance;
            row.CreatedAt = md.CreatedAt;
        }
    }
}
