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

    [UseProjection]
    public IQueryable<Invoice> GetMyInvoiceByNumber(
        GaneshaContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient,
        string invoiceNumber)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        return context.Invoices.Where(i => i.UserId == userId && i.InvoiceNumber == invoiceNumber);
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

    [UseProjection]
    public BalanceSummary GetMyBalance(
        GaneshaContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;

        var transactions = context.Transactions.Where(t => t.UserId == userId);
        var lastTransaction = transactions
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.TransactionId)
            .FirstOrDefault();

        return new BalanceSummary
        {
            Balance = lastTransaction?.Balance ?? 0,
            TotalCredits = transactions.Where(t => t.Type == 1).Sum(t => t.Amount),
            TotalDebits = transactions.Where(t => t.Type == 2).Sum(t => t.Amount),
            TransactionCount = transactions.Count()
        };
    }
}

public class BalanceSummary
{
    public double Balance { get; set; }
    public double TotalCredits { get; set; }
    public double TotalDebits { get; set; }
    public int TransactionCount { get; set; }
}
