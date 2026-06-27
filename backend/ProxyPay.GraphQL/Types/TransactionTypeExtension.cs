using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using ProxyPay.Infra.Context;

namespace ProxyPay.GraphQL.Types;

[ExtendObjectType(typeof(Transaction))]
public class TransactionTypeExtension
{
    public Invoice GetInvoice(
        [Parent] Transaction transaction,
        ProxyPayContext context)
    {
        if (transaction.InvoiceId == null)
            return null;

        return context.Invoices.FirstOrDefault(i => i.InvoiceId == transaction.InvoiceId);
    }
}
