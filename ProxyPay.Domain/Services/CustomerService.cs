using AutoMapper;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository<CustomerModel> _customerRepository;
        private readonly IMapper _mapper;

        public CustomerService(
            ICustomerRepository<CustomerModel> customerRepository,
            IMapper mapper
        )
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
        }

        public async Task<CustomerModel> GetByIdAsync(long customerId)
        {
            return await _customerRepository.GetByIdAsync(customerId);
        }

        public Task<CustomerInfo> GetCustomerInfoAsync(CustomerModel model)
        {
            var info = _mapper.Map<CustomerInfo>(model);
            return Task.FromResult(info);
        }

        public async Task<IList<CustomerInfo>> ListByStoreAsync(long storeId)
        {
            var customers = await _customerRepository.ListByStoreAsync(storeId);
            return customers.Select(c => _mapper.Map<CustomerInfo>(c)).ToList();
        }

        public async Task<CustomerModel> InsertAsync(CustomerInsertInfo customer, long storeId)
        {
            var model = _mapper.Map<CustomerModel>(customer);
            model.StoreId = storeId;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            return await _customerRepository.InsertAsync(model);
        }

        public async Task<CustomerModel> UpdateAsync(CustomerUpdateInfo customer)
        {
            var existing = await _customerRepository.GetByIdAsync(customer.CustomerId);
            if (existing == null)
                throw new Exception("Customer not found");

            _mapper.Map(customer, existing);
            existing.UpdatedAt = DateTime.Now;

            return await _customerRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(long customerId)
        {
            var existing = await _customerRepository.GetByIdAsync(customerId);
            if (existing == null)
                throw new Exception("Customer not found");

            await _customerRepository.DeleteAsync(customerId);
        }
    }
}
