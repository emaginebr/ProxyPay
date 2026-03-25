using ProxyPay.Domain.Models;
using ProxyPay.DTO.Customer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerModel> GetByIdAsync(long customerId);
        Task<CustomerInfo> GetCustomerInfoAsync(CustomerModel model);
        Task<IList<CustomerInfo>> ListByStoreAsync(long storeId);
        Task<CustomerModel> InsertAsync(CustomerInsertInfo customer, long storeId);
        Task<CustomerModel> UpdateAsync(CustomerUpdateInfo customer);
        Task DeleteAsync(long customerId);
    }
}
