using System.Linq;
using HotChocolate.Types;
using ProxyPay.Infra.Context;

namespace ProxyPay.GraphQL.Types;

public class StoreType : ObjectType<Store>
{
    protected override void Configure(IObjectTypeDescriptor<Store> descriptor)
    {
        // A credencial do AbacatePay é write-only: nunca é consultável pela API.
        descriptor.Field(s => s.AbacatePayApiKey).Ignore();

        // Indicador booleano (sem revelar o valor) de que a credencial está configurada.
        descriptor.Field("hasAbacatePayApiKey")
            .Type<NonNullType<BooleanType>>()
            .Resolve(context =>
            {
                var parent = context.Parent<Store>();
                var db = context.Service<ProxyPayContext>();
                return db.Stores
                    .Where(s => s.StoreId == parent.StoreId)
                    .Select(s => s.AbacatePayApiKey != null && s.AbacatePayApiKey != "")
                    .FirstOrDefault();
            });
    }
}
