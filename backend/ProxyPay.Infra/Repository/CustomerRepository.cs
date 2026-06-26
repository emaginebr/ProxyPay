using AutoMapper;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Infra.Context;
using ProxyPay.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyPay.Infra.Repository
{
    public class CustomerRepository : ICustomerRepository<CustomerModel>
    {
        private readonly ProxyPayContext _context;
        private readonly IMapper _mapper;

        public CustomerRepository(ProxyPayContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CustomerModel> InsertAsync(CustomerModel model)
        {
            var row = _mapper.Map<Customer>(model);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.CustomerId = row.CustomerId;
            return model;
        }

        public async Task<CustomerModel> UpdateAsync(CustomerModel model)
        {
            var row = await _context.Customers.FindAsync(model.CustomerId);
            _mapper.Map(model, row);
            _context.Customers.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<CustomerModel> GetByIdAsync(long id)
        {
            var row = await _context.Customers.FindAsync(id);
            if (row == null)
                return null;
            return _mapper.Map<CustomerModel>(row);
        }

        public async Task<CustomerModel> GetByEmailAndStoreAsync(string email, long storeId)
        {
            var row = await _context.Customers
                .Where(x => x.Email == email && x.StoreId == storeId)
                .FirstOrDefaultAsync();
            if (row == null)
                return null;
            return _mapper.Map<CustomerModel>(row);
        }

        public async Task<IEnumerable<CustomerModel>> ListByStoreAsync(long storeId)
        {
            var rows = await _context.Customers
                .Where(x => x.StoreId == storeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return rows.Select(r => _mapper.Map<CustomerModel>(r));
        }

        public async Task DeleteAsync(long id)
        {
            var row = await _context.Customers.FindAsync(id);
            if (row != null)
            {
                _context.Customers.Remove(row);
                await _context.SaveChangesAsync();
            }
        }
    }
}
