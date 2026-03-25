using HotChocolate.Execution.Configuration;
using HotChocolate.Execution.Options;
using HotChocolate.Types.Pagination;
using ProxyPay.GraphQL.Admin;
using ProxyPay.GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace ProxyPay.GraphQL;

public static class GraphQLServiceExtensions
{
    public static IServiceCollection AddProxyPayGraphQL(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddDiagnosticEventListener<GraphQLErrorLogger>()
            .AddErrorFilter<GraphQLErrorFilter>()
            .AddQueryType<AdminQuery>()
            .AddTypeExtension<InvoiceTypeExtension>()
            .AddTypeExtension<TransactionTypeExtension>()
            .ModifyPagingOptions(o =>
            {
                o.MaxPageSize = 50;
                o.DefaultPageSize = 10;
                o.IncludeTotalCount = true;
            })
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .ModifyCostOptions(o => o.MaxFieldCost = 8000);

        return services;
    }
}
