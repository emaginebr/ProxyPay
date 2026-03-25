using System.Linq;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using ProxyPay.Infra.Context;
using Microsoft.AspNetCore.Http;
using NAuth.ACL.Interfaces;

namespace ProxyPay.GraphQL.Admin;

[Authorize]
public class AdminQuery
{
    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Store> GetMyStores(
        ProxyPayContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        return context.Stores.Where(s => s.UserId == userId);
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Invoice> GetMyInvoices(
        ProxyPayContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        var storeIds = context.Stores.Where(s => s.UserId == userId).Select(s => s.StoreId);
        return context.Invoices.Where(i => i.StoreId != null && storeIds.Contains(i.StoreId.Value));
    }

    [UseProjection]
    public IQueryable<Invoice> GetMyInvoiceByNumber(
        ProxyPayContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient,
        string invoiceNumber)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        var storeIds = context.Stores.Where(s => s.UserId == userId).Select(s => s.StoreId);
        return context.Invoices.Where(i => i.StoreId != null && storeIds.Contains(i.StoreId.Value) && i.InvoiceNumber == invoiceNumber);
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Transaction> GetMyTransactions(
        ProxyPayContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        var storeIds = context.Stores.Where(s => s.UserId == userId).Select(s => s.StoreId);
        return context.Transactions.Where(t => t.StoreId != null && storeIds.Contains(t.StoreId.Value));
    }

    [UseProjection]
    public BalanceSummary GetMyBalance(
        ProxyPayContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        var storeIds = context.Stores.Where(s => s.UserId == userId).Select(s => s.StoreId);

        var transactions = context.Transactions.Where(t => t.StoreId != null && storeIds.Contains(t.StoreId.Value));
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

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Customer> GetMyCustomers(
        ProxyPayContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        var storeIds = context.Stores.Where(s => s.UserId == userId).Select(s => s.StoreId);
        return context.Customers.Where(c => c.StoreId != null && storeIds.Contains(c.StoreId.Value));
    }
}

public class BalanceSummary
{
    public double Balance { get; set; }
    public double TotalCredits { get; set; }
    public double TotalDebits { get; set; }
    public int TransactionCount { get; set; }
}
