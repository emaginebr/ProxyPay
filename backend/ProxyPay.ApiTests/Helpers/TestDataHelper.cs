using System;
using System.Collections.Generic;
using ProxyPay.DTO;
using ProxyPay.DTO.AbacatePay;
using ProxyPay.DTO.Billing;
using ProxyPay.DTO.Customer;
using ProxyPay.DTO.Invoice;
using ProxyPay.DTO.Store;

namespace ProxyPay.ApiTests.Helpers;

/// <summary>
/// Factories de payload para os testes de API. Uma Create&lt;DtoName&gt; por DTO
/// usado, com defaults sensatos. Sem factories órfãs.
/// </summary>
public static class TestDataHelper
{
    // ----------------------------------------------------------------- Customer
    /// <param name="withEmail">false produz um cliente sem email (caso de validação).</param>
    public static CustomerInsertInfo CreateCustomerInsertInfo(bool withEmail = true) => new()
    {
        Name = "QA Customer",
        DocumentId = "12345678909",
        Cellphone = "11999990000",
        Email = withEmail ? "qa.customer@proxypay.test" : string.Empty
    };

    // ------------------------------------------------------------------ Billing
    public static BillingRequest CreateBillingRequest(string clientId, CustomerInsertInfo? customer = null) => new()
    {
        ClientId = clientId,
        Frequency = BillingFrequencyEnum.Monthly,
        PaymentMethod = PaymentMethodEnum.Pix,
        BillingStartDate = DateTime.UtcNow.Date.AddDays(1),
        CompletionUrl = "https://example.com/completion",
        ReturnUrl = "https://example.com/return",
        Customer = customer is null ? CreateCustomerInsertInfo() : customer,
        Items = new List<BillingItemInfo>
        {
            new() { Description = "Plano QA", Quantity = 1, UnitPrice = 49.90, Discount = 0 }
        }
    };

    // ------------------------------------------------------------------ Invoice
    public static InvoiceRequest CreateInvoiceRequest(string clientId, CustomerInsertInfo? customer = null) => new()
    {
        ClientId = clientId,
        Customer = customer is null ? CreateCustomerInsertInfo() : customer,
        PaymentMethod = PaymentMethodEnum.Pix,
        CompletionUrl = "https://example.com/completion",
        ReturnUrl = "https://example.com/return",
        Notes = "Fatura QA",
        Discount = 0,
        DueDate = DateTime.UtcNow.Date.AddDays(7),
        Items = CreateInvoiceItems()
    };

    // ------------------------------------------------------------------- QRCode
    public static QRCodeRequest CreateQRCodeRequest(string clientId, CustomerInsertInfo? customer = null) => new()
    {
        ClientId = clientId,
        Customer = customer is null ? CreateCustomerInsertInfo() : customer,
        Items = CreateInvoiceItems()
    };

    private static IList<InvoiceItemRequest> CreateInvoiceItems() => new List<InvoiceItemRequest>
    {
        new() { Id = "item-1", Description = "Item QA", Quantity = 1, UnitPrice = 49.90, Discount = 0 }
    };

    // -------------------------------------------------------------------- Store
    public static StoreInsertInfo CreateStoreInsertInfo(string? name = null) => new()
    {
        Name = name ?? "QA Store",
        Email = "qa.store@proxypay.test",
        BillingStrategy = BillingStrategyEnum.Immediate
    };

    public static StoreUpdateInfo CreateStoreUpdateInfo(long storeId, string? name = null) => new()
    {
        StoreId = storeId,
        Name = name ?? "QA Store Updated",
        Email = "qa.store@proxypay.test",
        BillingStrategy = BillingStrategyEnum.Immediate
    };

    public static StoreApiKeyUpdateInfo CreateStoreApiKeyUpdateInfo(string? apiKey = null) => new()
    {
        ApiKey = apiKey ?? "abc_dev_qa_apikey_0123456789"
    };

    // ------------------------------------------------------------------ Webhook
    public static AbacatePayWebhookPayload CreateAbacatePayWebhookPayload(bool withData = true) => new()
    {
        Event = "billing.paid",
        DevMode = true,
        Data = withData
            ? new AbacatePayWebhookData
            {
                Id = "qa-webhook-data-id",
                Amount = 4990,
                Status = "PAID",
                UpdatedAt = DateTime.UtcNow.ToString("o")
            }
            : null
    };
}
