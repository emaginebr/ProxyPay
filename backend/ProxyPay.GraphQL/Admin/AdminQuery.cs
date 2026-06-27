using System.Linq;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using ProxyPay.DTO.GraphQL;
using ProxyPay.Infra.Context;
using Microsoft.AspNetCore.Http;
using NAuth.ACL.Interfaces;

namespace ProxyPay.GraphQL.Admin;

[Authorize]
public class AdminQuery
{
    private static long GetUserStoreId(ProxyPayContext context, IHttpContextAccessor httpContextAccessor, IUserClient userClient)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        var store = context.Stores.FirstOrDefault(s => s.UserId == userId);
        return store?.StoreId ?? 0;
    }

    [UseProjection]
    public IQueryable<Store> GetMyStore(
        ProxyPayContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var storeId = GetUserStoreId(context, httpContextAccessor, userClient);
        return context.Stores.Where(s => s.StoreId == storeId);
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
        var storeId = GetUserStoreId(context, httpContextAccessor, userClient);
        return context.Invoices.Where(i => i.StoreId == storeId);
    }

    [UseProjection]
    public IQueryable<Invoice> GetMyInvoiceByNumber(
        ProxyPayContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient,
        string invoiceNumber)
    {
        var storeId = GetUserStoreId(context, httpContextAccessor, userClient);
        return context.Invoices.Where(i => i.StoreId == storeId && i.InvoiceNumber == invoiceNumber);
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
        var storeId = GetUserStoreId(context, httpContextAccessor, userClient);
        return context.Transactions.Where(t => t.StoreId == storeId);
    }

    [UseProjection]
    public BalanceSummary GetMyBalance(
        ProxyPayContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var storeId = GetUserStoreId(context, httpContextAccessor, userClient);

        var transactions = context.Transactions.Where(t => t.StoreId == storeId);
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
        var storeId = GetUserStoreId(context, httpContextAccessor, userClient);
        return context.Customers.Where(c => c.StoreId == storeId);
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Billing> GetMyBillings(
        ProxyPayContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var storeId = GetUserStoreId(context, httpContextAccessor, userClient);
        return context.Billings.Where(b => b.StoreId == storeId);
    }
}
