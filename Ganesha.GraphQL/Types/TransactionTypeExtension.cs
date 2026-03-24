using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using Ganesha.Infra.Context;

namespace Ganesha.GraphQL.Types;

[ExtendObjectType(typeof(Transaction))]
public class TransactionTypeExtension
{
    public Invoice GetInvoice(
        [Parent] Transaction transaction,
        GaneshaContext context)
    {
        if (transaction.InvoiceId == null)
            return null;

        return context.Invoices.FirstOrDefault(i => i.InvoiceId == transaction.InvoiceId);
    }
}
