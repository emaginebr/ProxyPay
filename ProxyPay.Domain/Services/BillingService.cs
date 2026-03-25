using AutoMapper;
using ProxyPay.Infra.Interfaces.Repository;
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
        private readonly ICustomerRepository<CustomerModel> _customerRepository;
        private readonly IMapper _mapper;

        public BillingService(
            IBillingRepository<BillingModel> billingRepository,
            ICustomerRepository<CustomerModel> customerRepository,
            IMapper mapper
        )
        {
            _billingRepository = billingRepository;
            _customerRepository = customerRepository;
            _mapper = mapper;
        }

        public async Task<BillingModel> GetByIdAsync(long billingId)
        {
            return await _billingRepository.GetByIdAsync(billingId);
        }

        public async Task<BillingInfo> GetBillingInfoAsync(BillingModel model)
        {
            var info = _mapper.Map<BillingInfo>(model);
            if (model.CustomerId.HasValue)
            {
                var customer = await _customerRepository.GetByIdAsync(model.CustomerId.Value);
                if (customer != null)
                    info.Customer = _mapper.Map<CustomerInfo>(customer);
            }
            return info;
        }

        public async Task<IList<BillingInfo>> ListByStoreAsync(long storeId)
        {
            var billings = await _billingRepository.ListByStoreAsync(storeId);
            var result = new List<BillingInfo>();
            foreach (var billing in billings)
            {
                result.Add(await GetBillingInfoAsync(billing));
            }
            return result;
        }

        public async Task<BillingModel> InsertAsync(BillingInsertInfo billing, long storeId)
        {
            if (billing.Customer == null)
                throw new Exception("Customer is required");

            if (string.IsNullOrWhiteSpace(billing.Customer.Email))
                throw new Exception("Customer email is required");

            var customerId = await UpsertCustomerAsync(billing.Customer, storeId);

            var model = _mapper.Map<BillingModel>(billing);
            model.StoreId = storeId;
            model.CustomerId = customerId;
            model.Status = BillingStatusEnum.AwaitingPayment;
            model.CreatedAt = DateTime.Now;

            return await _billingRepository.InsertAsync(model);
        }

        public async Task DeleteAsync(long billingId)
        {
            var existing = await _billingRepository.GetByIdAsync(billingId);
            if (existing == null)
                throw new Exception("Billing not found");

            await _billingRepository.DeleteAsync(billingId);
        }

        private async Task<long> UpsertCustomerAsync(CustomerInsertInfo customerInfo, long storeId)
        {
            var existing = await _customerRepository.GetByEmailAndStoreAsync(customerInfo.Email, storeId);

            if (existing != null)
            {
                _mapper.Map(customerInfo, existing);
                existing.UpdatedAt = DateTime.Now;
                await _customerRepository.UpdateAsync(existing);
                return existing.CustomerId;
            }

            var newCustomer = _mapper.Map<CustomerModel>(customerInfo);
            newCustomer.StoreId = storeId;
            newCustomer.CreatedAt = DateTime.Now;
            newCustomer.UpdatedAt = DateTime.Now;

            var saved = await _customerRepository.InsertAsync(newCustomer);
            return saved.CustomerId;
        }
    }
}
