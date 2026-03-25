using HotChocolate;
using Microsoft.Extensions.Logging;

namespace ProxyPay.GraphQL;

public class GraphQLErrorFilter : IErrorFilter
{
    private readonly ILogger<GraphQLErrorFilter> _logger;

    public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger)
    {
        _logger = logger;
    }

    public IError OnError(IError error)
    {
        _logger.LogError(error.Exception, "GraphQL error: {Message} | Path: {Path}",
            error.Message,
            error.Path?.ToString() ?? "N/A");

        return error.WithMessage(error.Exception?.Message ?? error.Message);
    }
}
