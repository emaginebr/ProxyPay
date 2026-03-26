using AutoMapper;
using FluentValidation;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Infra.Interfaces.AppServices;
using ProxyPay.Domain.Core;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO;
using ProxyPay.DTO.AbacatePay;
using ProxyPay.DTO.Billing;
using ProxyPay.DTO.Customer;
using ProxyPay.DTO.Invoice;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Services
{
    public class BillingService : IBillingService
    {
        private readonly IBillingRepository<BillingModel> _billingRepository;
        private readonly IBillingItemRepository<BillingItemModel> _billingItemRepository;
        private readonly ICustomerRepository<CustomerModel> _customerRepository;
        private readonly IAbacatePayAppService _abacatePayAppService;
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceRepository<InvoiceModel> _invoiceRepository;
        private readonly IValidator<BillingRequest> _billingRequestValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<BillingService> _logger;

        public BillingService(
            IBillingRepository<BillingModel> billingRepository,
            IBillingItemRepository<BillingItemModel> billingItemRepository,
            ICustomerRepository<CustomerModel> customerRepository,
            IAbacatePayAppService abacatePayAppService,
            IInvoiceService invoiceService,
            IInvoiceRepository<InvoiceModel> invoiceRepository,
            IValidator<BillingRequest> billingRequestValidator,
            IMapper mapper,
            ILogger<BillingService> logger
        )
        {
            _billingRepository = billingRepository;
            _billingItemRepository = billingItemRepository;
            _customerRepository = customerRepository;
            _abacatePayAppService = abacatePayAppService;
            _invoiceService = invoiceService;
            _invoiceRepository = invoiceRepository;
            _billingRequestValidator = billingRequestValidator;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BillingModel> GetByIdAsync(long billingId)
        {
            var model = await _billingRepository.GetByIdAsync(billingId);
            if (model == null)
                return null;

            var items = await _billingItemRepository.ListByBillingAsync(billingId);
            model.Items = items.ToList();

            return model;
        }

        public async Task<DTO.Billing.BillingInfo> GetBillingInfoAsync(BillingModel model)
        {
            var info = _mapper.Map<DTO.Billing.BillingInfo>(model);
            info.Items = model.Items.Select(i => _mapper.Map<BillingItemInfo>(i)).ToList();

            if (model.CustomerId.HasValue)
            {
                var customer = await _customerRepository.GetByIdAsync(model.CustomerId.Value);
                if (customer != null)
                    info.Customer = _mapper.Map<CustomerInfo>(customer);
            }

            return info;
        }

        public async Task<BillingResponse> CreateBillingAsync(BillingRequest request, long storeId, long customerId)
        {
            _logger.LogInformation("CreateBilling: validating input for store {StoreId}", storeId);

            _billingRequestValidator.ValidateAndThrow(request);

            var products = request.Items.Select(i => new BillingProductRequest
            {
                ExternalId = i.BillingItemId > 0 ? i.BillingItemId.ToString() : Guid.NewGuid().ToString("N"),
                Name = i.Description,
                Description = i.Description,
                Quantity = i.Quantity,
                Price = (int)(i.UnitPrice * 100)
            }).ToList();

            _logger.LogInformation("CreateBilling: calling AbacatePay Billing API with frequency MULTIPLE_PAYMENTS");

            var billingCreateRequest = new BillingCreateRequest
            {
                Frequency = AbacatePayBillingFrequency.MULTIPLE_PAYMENTS,
                Methods = new List<string> { "CARD" },
                Products = products,
                ReturnUrl = request.ReturnUrl,
                CompletionUrl = request.CompletionUrl,
                Customer = new AbacatePayCustomerRequest
                {
                    Name = request.Customer.Name,
                    Email = request.Customer.Email,
                    Cellphone = request.Customer.Cellphone,
                    TaxId = Utils.CleanCpf(request.Customer.DocumentId)
                }
            };

            var abacatePayResponse = await _abacatePayAppService.CreateBillingAsync(billingCreateRequest);

            if (abacatePayResponse?.Data == null)
                throw new Exception("Failed to create billing: no response from payment provider");

            var billingData = abacatePayResponse.Data;
            _logger.LogInformation("CreateBilling: AbacatePay billing created with ID {AbacatePayId}, URL: {Url}", billingData.Id, billingData.Url);

            _logger.LogInformation("CreateBilling: creating billing for store {StoreId}", storeId);

            var savedBilling = await InsertAsync(request, storeId);

            _logger.LogInformation("CreateBilling: creating invoice for billing {BillingId}", savedBilling.BillingId);

            var invoiceRequest = new InvoiceRequest
            {
                ClientId = request.ClientId,
                Customer = request.Customer,
                PaymentMethod = request.PaymentMethod,
                Discount = 0,
                DueDate = request.BillingStartDate,
                Items = request.Items.Select(i => new InvoiceItemRequest
                {
                    Id = i.BillingItemId > 0 ? i.BillingItemId.ToString() : Guid.NewGuid().ToString("N"),
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount
                }).ToList()
            };

            var savedInvoice = await _invoiceService.InsertAsync(invoiceRequest, storeId, customerId);
            savedInvoice.ExternalCode = billingData.Id;
            await _invoiceRepository.UpdateAsync(savedInvoice);

            _logger.LogInformation("CreateBilling: billing {BillingId} and invoice {InvoiceId} created successfully",
                savedBilling.BillingId, savedInvoice.InvoiceId);

            return new BillingResponse
            {
                BillingId = savedBilling.BillingId,
                InvoiceId = savedInvoice.InvoiceId,
                InvoiceNumber = savedInvoice.InvoiceNumber,
                Url = billingData.Url
            };
        }

        public async Task<BillingModel> InsertAsync(BillingRequest billing, long storeId)
        {
            if (billing.Customer == null)
                throw new Exception("Customer is required");

            if (string.IsNullOrWhiteSpace(billing.Customer.Email))
                throw new Exception("Customer email is required");

            if (string.IsNullOrWhiteSpace(billing.Customer.DocumentId))
                throw new Exception("Customer CPF (documentId) is required");

            if (!Utils.IsValidCpf(billing.Customer.DocumentId))
                throw new Exception("Customer CPF (documentId) is invalid");

            var customerId = await UpsertCustomerAsync(billing.Customer, storeId);

            var model = _mapper.Map<BillingModel>(billing);
            model.StoreId = storeId;
            model.CustomerId = customerId;
            model.Status = BillingStatusEnum.AwaitingPayment;
            model.CreatedAt = DateTime.Now;

            var saved = await _billingRepository.InsertAsync(model);

            if (billing.Items != null)
            {
                foreach (var itemInfo in billing.Items)
                {
                    var itemModel = _mapper.Map<BillingItemModel>(itemInfo);
                    itemModel.BillingId = saved.BillingId;
                    itemModel.CreatedAt = DateTime.Now;
                    var savedItem = await _billingItemRepository.InsertAsync(itemModel);
                    saved.Items.Add(savedItem);
                }
            }

            return saved;
        }

        public async Task<BillingModel> UpdateAsync(long billingId, BillingRequest billing)
        {
            var existing = await GetByIdAsync(billingId);
            if (existing == null)
                throw new Exception("Billing not found");

            existing.UpdateFrequency(billing.Frequency);
            existing.BillingStartDate = billing.BillingStartDate;

            await _billingRepository.UpdateAsync(existing);

            await _billingItemRepository.DeleteByBillingAsync(billingId);
            existing.Items.Clear();

            if (billing.Items != null)
            {
                foreach (var itemInfo in billing.Items)
                {
                    var itemModel = _mapper.Map<BillingItemModel>(itemInfo);
                    itemModel.BillingId = billingId;
                    itemModel.CreatedAt = DateTime.Now;
                    var savedItem = await _billingItemRepository.InsertAsync(itemModel);
                    existing.Items.Add(savedItem);
                }
            }

            return existing;
        }

        public async Task DeleteAsync(long billingId)
        {
            var existing = await _billingRepository.GetByIdAsync(billingId);
            if (existing == null)
                throw new Exception("Billing not found");

            await _billingItemRepository.DeleteByBillingAsync(billingId);
            await _billingRepository.DeleteAsync(billingId);
        }

        private async Task<long> UpsertCustomerAsync(CustomerInsertInfo customerInfo, long storeId)
        {
            var existing = await _customerRepository.GetByEmailAndStoreAsync(customerInfo.Email, storeId);

            if (existing != null)
            {
                _mapper.Map(customerInfo, existing);
                existing.MarkUpdated();
                await _customerRepository.UpdateAsync(existing);
                return existing.CustomerId;
            }

            var newCustomer = _mapper.Map<CustomerModel>(customerInfo);
            newCustomer.SetStore(storeId);
            newCustomer.MarkCreated();

            var saved = await _customerRepository.InsertAsync(newCustomer);
            return saved.CustomerId;
        }
    }
}
