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
    public class BillingItemRepository : IBillingItemRepository<BillingItemModel>
    {
        private readonly ProxyPayContext _context;
        private readonly IMapper _mapper;

        public BillingItemRepository(ProxyPayContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BillingItemModel> InsertAsync(BillingItemModel model)
        {
            var row = _mapper.Map<BillingItem>(model);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.BillingItemId = row.BillingItemId;
            return model;
        }

        public async Task<BillingItemModel> UpdateAsync(BillingItemModel model)
        {
            var row = await _context.BillingItems.FindAsync(model.BillingItemId);
            _mapper.Map(model, row);
            _context.BillingItems.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<BillingItemModel> GetByIdAsync(long id)
        {
            var row = await _context.BillingItems.FindAsync(id);
            if (row == null)
                return null;
            return _mapper.Map<BillingItemModel>(row);
        }

        public async Task<IEnumerable<BillingItemModel>> ListByBillingAsync(long billingId)
        {
            var rows = await _context.BillingItems
                .Where(x => x.BillingId == billingId)
                .ToListAsync();
            return rows.Select(r => _mapper.Map<BillingItemModel>(r));
        }

        public async Task DeleteAsync(long id)
        {
            var row = await _context.BillingItems.FindAsync(id);
            if (row != null)
            {
                _context.BillingItems.Remove(row);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByBillingAsync(long billingId)
        {
            var rows = await _context.BillingItems
                .Where(x => x.BillingId == billingId)
                .ToListAsync();
            _context.BillingItems.RemoveRange(rows);
            await _context.SaveChangesAsync();
        }
    }
}
