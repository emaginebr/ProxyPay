using HotChocolate.Execution.Configuration;
using HotChocolate.Types.Pagination;
using Ganesha.GraphQL.Admin;
using Ganesha.GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Ganesha.GraphQL;

public static class GraphQLServiceExtensions
{
    public static IServiceCollection AddGaneshaGraphQL(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddDiagnosticEventListener<GraphQLErrorLogger>()
            .AddQueryType<AdminQuery>()
            .AddTypeExtension<InvoiceTypeExtension>()
            .SetPagingOptions(new PagingOptions
            {
                MaxPageSize = 50,
                DefaultPageSize = 10,
                IncludeTotalCount = true
            })
            .AddProjections()
            .AddFiltering()
            .AddSorting();

        return services;
    }
}
