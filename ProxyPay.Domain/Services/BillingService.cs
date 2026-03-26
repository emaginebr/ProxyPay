using AutoMapper;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Domain.Core;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO.Billing;
using ProxyPay.DTO.Customer;
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
        private readonly IMapper _mapper;

        public BillingService(
            IBillingRepository<BillingModel> billingRepository,
            IBillingItemRepository<BillingItemModel> billingItemRepository,
            ICustomerRepository<CustomerModel> customerRepository,
            IMapper mapper
        )
        {
            _billingRepository = billingRepository;
            _billingItemRepository = billingItemRepository;
            _customerRepository = customerRepository;
            _mapper = mapper;
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

        public async Task<BillingInfo> GetBillingInfoAsync(BillingModel model)
        {
            var info = _mapper.Map<BillingInfo>(model);
            info.Items = model.Items.Select(i => _mapper.Map<BillingItemInfo>(i)).ToList();

            if (model.CustomerId.HasValue)
            {
                var customer = await _customerRepository.GetByIdAsync(model.CustomerId.Value);
                if (customer != null)
                    info.Customer = _mapper.Map<CustomerInfo>(customer);
            }

            return info;
        }

        public async Task<BillingModel> InsertAsync(BillingInsertInfo billing, long storeId)
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

        public async Task<BillingModel> UpdateAsync(long billingId, BillingInsertInfo billing)
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
