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
    public class BillingRepository : IBillingRepository<BillingModel>
    {
        private readonly ProxyPayContext _context;
        private readonly IMapper _mapper;

        public BillingRepository(ProxyPayContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BillingModel> InsertAsync(BillingModel model)
        {
            var row = _mapper.Map<Billing>(model);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.BillingId = row.BillingId;
            return model;
        }

        public async Task<BillingModel> UpdateAsync(BillingModel model)
        {
            var row = await _context.Billings.FindAsync(model.BillingId);
            _mapper.Map(model, row);
            _context.Billings.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<BillingModel> GetByIdAsync(long id)
        {
            var row = await _context.Billings.FindAsync(id);
            if (row == null)
                return null;
            return _mapper.Map<BillingModel>(row);
        }

        public async Task<IEnumerable<BillingModel>> ListByStoreAsync(long storeId)
        {
            var rows = await _context.Billings
                .Where(x => x.StoreId == storeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return rows.Select(r => _mapper.Map<BillingModel>(r));
        }

        public async Task DeleteAsync(long id)
        {
            var row = await _context.Billings.FindAsync(id);
            if (row != null)
            {
                _context.Billings.Remove(row);
                await _context.SaveChangesAsync();
            }
        }
    }
}
