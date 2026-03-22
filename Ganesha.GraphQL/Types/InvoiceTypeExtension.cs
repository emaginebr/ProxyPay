using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using Ganesha.Infra.Context;

namespace Ganesha.GraphQL.Types;

[ExtendObjectType(typeof(Invoice))]
public class InvoiceTypeExtension
{
    public IQueryable<InvoiceItem> GetItems(
        [Parent] Invoice invoice,
        GaneshaContext context)
    {
        return context.InvoiceItems.Where(i => i.InvoiceId == invoice.InvoiceId);
    }
}
