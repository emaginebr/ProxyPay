using System.Linq;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Ganesha.Infra.Context;
using Microsoft.AspNetCore.Http;
using NAuth.ACL.Interfaces;

namespace Ganesha.GraphQL.Admin;

[Authorize]
public class AdminQuery
{
    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Invoice> GetMyInvoices(
        GaneshaContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        return context.Invoices.Where(i => i.UserId == userId);
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Transaction> GetMyTransactions(
        GaneshaContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        return context.Transactions.Where(t => t.UserId == userId);
    }

}
