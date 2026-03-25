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
    public class StoreRepository : IStoreRepository<StoreModel>
    {
        private readonly ProxyPayContext _context;
        private readonly IMapper _mapper;

        public StoreRepository(ProxyPayContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<StoreModel> InsertAsync(StoreModel model)
        {
            var row = _mapper.Map<Store>(model);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.StoreId = row.StoreId;
            return model;
        }

        public async Task<StoreModel> UpdateAsync(StoreModel model)
        {
            var row = await _context.Stores.FindAsync(model.StoreId);
            _mapper.Map(model, row);
            _context.Stores.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<StoreModel> GetByIdAsync(long id)
        {
            var row = await _context.Stores.FindAsync(id);
            if (row == null)
                return null;
            return _mapper.Map<StoreModel>(row);
        }

        public async Task<IEnumerable<StoreModel>> ListByUserAsync(long userId)
        {
            var rows = await _context.Stores
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return rows.Select(r => _mapper.Map<StoreModel>(r));
        }

        public async Task DeleteAsync(long id)
        {
            var row = await _context.Stores.FindAsync(id);
            if (row != null)
            {
                _context.Stores.Remove(row);
                await _context.SaveChangesAsync();
            }
        }
    }
}
