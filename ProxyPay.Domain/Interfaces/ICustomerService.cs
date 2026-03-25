using ProxyPay.Domain.Models;
using ProxyPay.DTO.Customer;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerModel> GetByIdAsync(long customerId);
        Task<CustomerModel> InsertAsync(CustomerInsertInfo customer, long storeId);
        Task<long> UpsertAsync(CustomerInsertInfo customer, long storeId);
        Task<CustomerModel> UpdateAsync(CustomerUpdateInfo customer);
        Task DeleteAsync(long customerId);
    }
}
