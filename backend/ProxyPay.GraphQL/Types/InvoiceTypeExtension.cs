using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using ProxyPay.Infra.Context;

namespace ProxyPay.GraphQL.Types;

[ExtendObjectType(typeof(Invoice))]
public class InvoiceTypeExtension
{
    public IQueryable<InvoiceItem> GetItems(
        [Parent] Invoice invoice,
        ProxyPayContext context)
    {
        return context.InvoiceItems.Where(i => i.InvoiceId == invoice.InvoiceId);
    }
}
