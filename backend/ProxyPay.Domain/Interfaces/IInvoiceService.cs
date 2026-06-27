using ProxyPay.Domain.Models;
using ProxyPay.DTO.AbacatePay;
using ProxyPay.DTO.Invoice;
using System;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceModel> GetByIdAsync(long invoiceId);
        Task<InvoiceInfo> GetInvoiceInfoAsync(InvoiceModel model);
        Task<InvoiceModel> InsertAsync(InvoiceRequest invoice, long storeId);
        Task<InvoiceModel> InsertAsync(InvoiceRequest invoice, long storeId, long customerId);
        Task<InvoiceResponse> CreateInvoiceAsync(InvoiceRequest request, long storeId, long customerId);
        Task<QRCodeResponse> CreateQRCodeAsync(QRCodeRequest request, long storeId, long customerId);
        Task<QRCodeStatusResponse> CheckQRCodeStatusAsync(long invoiceId);
        Task<AbacatePayResponse<PixQrCodeInfo>> SimulatePaymentAsync(long invoiceId);
        Task<InvoiceModel> MarkAsPaidAsync(long invoiceId, DateTime? paidAt = null);
        Task<InvoiceModel> MarkAsExpiredAsync(long invoiceId);
        Task<InvoiceModel> CancelAsync(long invoiceId);
        Task ProcessWebhookAsync(string eventType, string externalCode, DateTime? updatedAt);
        Task<InvoiceModel> UpdateAsync(InvoiceUpdateInfo invoice);
        Task DeleteAsync(long invoiceId);
    }
}
