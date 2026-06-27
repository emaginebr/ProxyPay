using AutoMapper;
using FluentValidation;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Infra.Interfaces.AppServices;
using ProxyPay.Domain.Core;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO;
using ProxyPay.DTO.AbacatePay;
using ProxyPay.DTO.Invoice;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository<InvoiceModel> _invoiceRepository;
        private readonly IInvoiceItemRepository<InvoiceItemModel> _invoiceItemRepository;
        private readonly IAbacatePayAppService _abacatePayAppService;
        private readonly IValidator<InvoiceRequest> _invoiceInsertValidator;
        private readonly IValidator<QRCodeRequest> _qrCodeRequestValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(
            IInvoiceRepository<InvoiceModel> invoiceRepository,
            IInvoiceItemRepository<InvoiceItemModel> invoiceItemRepository,
            IAbacatePayAppService abacatePayAppService,
            IValidator<InvoiceRequest> invoiceInsertValidator,
            IValidator<QRCodeRequest> qrCodeRequestValidator,
            IMapper mapper,
            ILogger<InvoiceService> logger
        )
        {
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _abacatePayAppService = abacatePayAppService;
            _invoiceInsertValidator = invoiceInsertValidator;
            _qrCodeRequestValidator = qrCodeRequestValidator;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<InvoiceModel> GetByIdAsync(long invoiceId)
        {
            var model = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (model == null)
                return null;

            var items = await _invoiceItemRepository.ListByInvoiceAsync(invoiceId);
            model.Items = items.ToList();

            return model;
        }

        public async Task<InvoiceInfo> GetInvoiceInfoAsync(InvoiceModel model)
        {
            var info = _mapper.Map<InvoiceInfo>(model);
            info.Items = model.Items.Select(i => _mapper.Map<InvoiceItemInfo>(i)).ToList();
            return info;
        }

        public async Task<InvoiceModel> InsertAsync(InvoiceRequest invoice, long storeId, long customerId)
        {
            var model = await InsertAsync(invoice, storeId);
            model.SetCustomer(customerId);
            await _invoiceRepository.UpdateAsync(model);
            return model;
        }

        public async Task<InvoiceModel> InsertAsync(InvoiceRequest invoice, long storeId)
        {
            _invoiceInsertValidator.ValidateAndThrow(invoice);

            var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(storeId);

            var model = _mapper.Map<InvoiceModel>(invoice);
            model.SetStore(storeId);
            model.SetInvoiceNumber(invoiceNumber);
            model.MarkAsPending();
            model.MarkCreated();

            var savedInvoice = await _invoiceRepository.InsertAsync(model);

            foreach (var item in invoice.Items)
            {
                var itemModel = _mapper.Map<InvoiceItemModel>(item);
                itemModel.InvoiceId = savedInvoice.InvoiceId;
                itemModel.MarkCreated();
                var savedItem = await _invoiceItemRepository.InsertAsync(itemModel);
                savedInvoice.Items.Add(savedItem);
            }

            return savedInvoice;
        }

        public async Task<InvoiceResponse> CreateInvoiceAsync(InvoiceRequest request, long storeId, long customerId)
        {
            _logger.LogInformation("CreateInvoice: validating input for store {StoreId}", storeId);

            _invoiceInsertValidator.ValidateAndThrow(request);

            var products = request.Items.Select(i => new BillingProductRequest
            {
                ExternalId = i.Id,
                Name = i.Description,
                Description = i.Description,
                Quantity = i.Quantity,
                Price = (int)(i.UnitPrice * 100)
            }).ToList();

            _logger.LogInformation("CreateInvoice: calling AbacatePay Billing API");

            var billingRequest = new BillingCreateRequest
            {
                Frequency = AbacatePayBillingFrequency.ONE_TIME,
                Methods = new List<string> { "CARD" },
                Products = products,
                ReturnUrl = request.ReturnUrl,
                CompletionUrl = request.CompletionUrl,
                Customer = new AbacatePayCustomerRequest
                {
                    Name = request.Customer.Name,
                    Email = request.Customer.Email,
                    Cellphone = request.Customer.Cellphone,
                    TaxId = Core.Utils.CleanCpf(request.Customer.DocumentId)
                }
            };

            var abacatePayResponse = await _abacatePayAppService.CreateBillingAsync(billingRequest);

            if (abacatePayResponse?.Data == null)
                throw new Exception("Failed to create billing: no response from payment provider");

            var billingData = abacatePayResponse.Data;
            _logger.LogInformation("CreateInvoice: billing created with ID {BillingId}, URL: {Url}", billingData.Id, billingData.Url);

            _logger.LogInformation("CreateInvoice: creating invoice for store {StoreId}", storeId);

            var savedInvoice = await InsertAsync(request, storeId, customerId);
            savedInvoice.ExternalCode = billingData.Id;
            await _invoiceRepository.UpdateAsync(savedInvoice);

            return new InvoiceResponse
            {
                InvoiceId = savedInvoice.InvoiceId,
                InvoiceNumber = savedInvoice.InvoiceNumber,
                Url = billingData.Url
            };
        }

        public async Task<QRCodeResponse> CreateQRCodeAsync(QRCodeRequest request, long storeId, long customerId)
        {
            _logger.LogInformation("CreateQRCode: validating input for store {StoreId}", storeId);

            _qrCodeRequestValidator.ValidateAndThrow(request);

            var totalAmount = request.Items.Sum(i => (i.Quantity * i.UnitPrice) - i.Discount);

            _logger.LogInformation("CreateQRCode: calling AbacatePay API for amount {Amount}", (int)(totalAmount * 100));

            var pixRequest = new PixQrCodeCreateRequest
            {
                Amount = (int)(totalAmount * 100),
                Description = "Payment",
                Customer = new AbacatePayCustomerRequest
                {
                    Name = request.Customer.Name,
                    Email = request.Customer.Email,
                    Cellphone = request.Customer.Cellphone,
                    TaxId = Utils.CleanCpf(request.Customer.DocumentId)
                }
            };

            var abacatePayResponse = await _abacatePayAppService.CreatePixQrCodeAsync(pixRequest);

            if (abacatePayResponse?.Data == null)
                throw new Exception("Failed to create QR Code: no response from payment provider");

            var qrCodeData = abacatePayResponse.Data;
            _logger.LogInformation("CreateQRCode: QR Code created with external ID {ExternalId}", qrCodeData.Id);

            _logger.LogInformation("CreateQRCode: creating invoice for store {StoreId}", storeId);

            var invoiceInsert = new InvoiceRequest
            {
                ClientId = request.ClientId,
                Customer = request.Customer,
                PaymentMethod = PaymentMethodEnum.Pix,
                Discount = 0,
                DueDate = DateTime.Now.AddDays(1),
                Items = request.Items
            };

            var savedInvoice = await InsertAsync(invoiceInsert, storeId, customerId);
            savedInvoice.PaymentMethod = PaymentMethodEnum.Pix;
            savedInvoice.ExternalCode = qrCodeData.Id;

            if (DateTime.TryParse(qrCodeData.ExpiresAt, out var expiresAt))
                savedInvoice.ExpiresAt = expiresAt;

            await _invoiceRepository.UpdateAsync(savedInvoice);

            _logger.LogInformation("CreateQRCode: invoice {InvoiceId} created with number {InvoiceNumber}",
                savedInvoice.InvoiceId, savedInvoice.InvoiceNumber);

            return new QRCodeResponse
            {
                InvoiceId = savedInvoice.InvoiceId,
                InvoiceNumber = savedInvoice.InvoiceNumber,
                BrCode = qrCodeData.BrCode,
                BrCodeBase64 = qrCodeData.BrCodeBase64,
                ExpiredAt = savedInvoice.ExpiresAt
            };
        }

        public async Task<QRCodeStatusResponse> CheckQRCodeStatusAsync(long invoiceId)
        {
            _logger.LogInformation("CheckQRCodeStatus: checking invoice {InvoiceId}", invoiceId);

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            if (string.IsNullOrWhiteSpace(invoice.ExternalCode))
                throw new Exception("Invoice does not have an associated QR Code");

            if (invoice.Status != InvoiceStatusEnum.Pending)
            {
                _logger.LogInformation("CheckQRCodeStatus: invoice {InvoiceId} already has status {Status}", invoiceId, invoice.Status);
                return BuildStatusResponse(invoice);
            }

            _logger.LogInformation("CheckQRCodeStatus: calling AbacatePay for external code {ExternalCode}", invoice.ExternalCode);

            var abacatePayResponse = await _abacatePayAppService.CheckStatusAsync(invoice.ExternalCode);

            if (abacatePayResponse?.Data == null)
                throw new Exception("Failed to check QR Code status: no response from payment provider");

            var status = abacatePayResponse.Data.Status?.ToUpper();
            _logger.LogInformation("CheckQRCodeStatus: AbacatePay status for invoice {InvoiceId}: {Status}", invoiceId, status);

            DateTime? paidAt = null;
            if (status == "PAID" && DateTime.TryParse(abacatePayResponse.Data.UpdatedAt, out var parsedPaidAt))
                paidAt = parsedPaidAt;

            switch (status)
            {
                case "PAID":
                    await MarkAsPaidAsync(invoiceId, paidAt);
                    invoice.MarkAsPaid(paidAt);
                    break;
                case "EXPIRED":
                    await MarkAsExpiredAsync(invoiceId);
                    invoice.MarkAsExpired();
                    break;
                case "CANCELLED":
                    await CancelAsync(invoiceId);
                    invoice.Cancel();
                    break;
                case "PENDING":
                default:
                    break;
            }

            return BuildStatusResponse(invoice);
        }

        public async Task<AbacatePayResponse<PixQrCodeInfo>> SimulatePaymentAsync(long invoiceId)
        {
            _logger.LogInformation("SimulatePayment: invoice {InvoiceId}", invoiceId);

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            if (string.IsNullOrWhiteSpace(invoice.ExternalCode))
                throw new Exception("Invoice does not have an associated QR Code");

            _logger.LogInformation("SimulatePayment: calling AbacatePay for external code {ExternalCode}", invoice.ExternalCode);

            var response = await _abacatePayAppService.SimulatePaymentAsync(invoice.ExternalCode);

            if (response?.Data == null)
                throw new Exception("Failed to simulate payment: no response from payment provider");

            return response;
        }

        public async Task<InvoiceModel> MarkAsPaidAsync(long invoiceId, DateTime? paidAt = null)
        {
            _logger.LogInformation("MarkAsPaid: invoice {InvoiceId}, paidAt {PaidAt}", invoiceId, paidAt);

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            invoice.MarkAsPaid(paidAt);
            return await _invoiceRepository.UpdateAsync(invoice);
        }

        public async Task<InvoiceModel> MarkAsExpiredAsync(long invoiceId)
        {
            _logger.LogInformation("MarkAsExpired: invoice {InvoiceId}", invoiceId);

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            invoice.MarkAsExpired();
            return await _invoiceRepository.UpdateAsync(invoice);
        }

        public async Task<InvoiceModel> CancelAsync(long invoiceId)
        {
            _logger.LogInformation("Cancel: invoice {InvoiceId}", invoiceId);

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            invoice.Cancel();
            return await _invoiceRepository.UpdateAsync(invoice);
        }

        private static QRCodeStatusResponse BuildStatusResponse(InvoiceModel invoice)
        {
            return new QRCodeStatusResponse
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                Paid = invoice.Status == InvoiceStatusEnum.Paid,
                Status = invoice.Status,
                StatusText = invoice.Status.ToString(),
                ExpiresAt = invoice.ExpiresAt
            };
        }

        public async Task ProcessWebhookAsync(string eventType, string externalCode, DateTime? updatedAt)
        {
            _logger.LogInformation("ProcessWebhook: event={Event}, externalCode={ExternalCode}", eventType, externalCode);

            var invoice = await _invoiceRepository.GetByExternalCodeAsync(externalCode);
            if (invoice == null)
            {
                _logger.LogWarning("ProcessWebhook: no invoice found for externalCode {ExternalCode}", externalCode);
                return;
            }

            _logger.LogInformation("ProcessWebhook: found invoice {InvoiceId} with status {Status}", invoice.InvoiceId, invoice.Status);

            switch (eventType)
            {
                case "billing.paid":
                    if (invoice.Status == InvoiceStatusEnum.Paid)
                    {
                        _logger.LogInformation("ProcessWebhook: invoice {InvoiceId} already paid, skipping", invoice.InvoiceId);
                        return;
                    }
                    await MarkAsPaidAsync(invoice.InvoiceId, updatedAt);
                    _logger.LogInformation("ProcessWebhook: invoice {InvoiceId} marked as paid", invoice.InvoiceId);
                    break;

                case "billing.refunded":
                    if (invoice.Status == InvoiceStatusEnum.Cancelled)
                    {
                        _logger.LogInformation("ProcessWebhook: invoice {InvoiceId} already cancelled, skipping", invoice.InvoiceId);
                        return;
                    }
                    await CancelAsync(invoice.InvoiceId);
                    _logger.LogInformation("ProcessWebhook: invoice {InvoiceId} cancelled (refunded)", invoice.InvoiceId);
                    break;

                case "billing.failed":
                    if (invoice.Status == InvoiceStatusEnum.Expired)
                    {
                        _logger.LogInformation("ProcessWebhook: invoice {InvoiceId} already expired, skipping", invoice.InvoiceId);
                        return;
                    }
                    await MarkAsExpiredAsync(invoice.InvoiceId);
                    _logger.LogInformation("ProcessWebhook: invoice {InvoiceId} marked as expired (failed)", invoice.InvoiceId);
                    break;

                case "billing.created":
                    _logger.LogInformation("ProcessWebhook: billing.created for invoice {InvoiceId}, no action needed", invoice.InvoiceId);
                    break;

                default:
                    _logger.LogWarning("ProcessWebhook: unhandled event {Event} for invoice {InvoiceId}", eventType, invoice.InvoiceId);
                    break;
            }
        }

        public async Task<InvoiceModel> UpdateAsync(InvoiceUpdateInfo invoice)
        {
            var existing = await GetByIdAsync(invoice.InvoiceId);
            if (existing == null)
                throw new Exception("Invoice not found");

            if (invoice.Items == null || !invoice.Items.Any())
                throw new Exception("Invoice must have at least one item");

            existing.Notes = invoice.Notes;
            existing.Discount = invoice.Discount;
            existing.DueDate = invoice.DueDate;
            existing.SetStatus(invoice.Status);

            var updated = await _invoiceRepository.UpdateAsync(existing);

            await _invoiceItemRepository.DeleteByInvoiceAsync(existing.InvoiceId);
            updated.ClearItems();

            foreach (var item in invoice.Items)
            {
                var itemModel = _mapper.Map<InvoiceItemModel>(item);
                itemModel.InvoiceId = existing.InvoiceId;
                itemModel.MarkCreated();
                var savedItem = await _invoiceItemRepository.InsertAsync(itemModel);
                updated.Items.Add(savedItem);
            }

            return updated;
        }

        public async Task DeleteAsync(long invoiceId)
        {
            var existing = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (existing == null)
                throw new Exception("Invoice not found");

            await _invoiceItemRepository.DeleteByInvoiceAsync(invoiceId);
            await _invoiceRepository.DeleteAsync(invoiceId);
        }
    }
}
