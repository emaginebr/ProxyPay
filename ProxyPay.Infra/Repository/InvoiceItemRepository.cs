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
    public class InvoiceItemRepository : IInvoiceItemRepository<InvoiceItemModel>
    {
        private readonly ProxyPayContext _context;
        private readonly IMapper _mapper;

        public InvoiceItemRepository(ProxyPayContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<InvoiceItemModel> InsertAsync(InvoiceItemModel model)
        {
            var row = _mapper.Map<InvoiceItem>(model);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.InvoiceItemId = row.InvoiceItemId;
            return model;
        }

        public async Task<InvoiceItemModel> UpdateAsync(InvoiceItemModel model)
        {
            var row = await _context.InvoiceItems.FindAsync(model.InvoiceItemId);
            _mapper.Map(model, row);
            _context.InvoiceItems.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<InvoiceItemModel> GetByIdAsync(long id)
        {
            var row = await _context.InvoiceItems.FindAsync(id);
            if (row == null)
                return null;
            return _mapper.Map<InvoiceItemModel>(row);
        }

        public async Task<IEnumerable<InvoiceItemModel>> ListByInvoiceAsync(long invoiceId)
        {
            var rows = await _context.InvoiceItems
                .Where(x => x.InvoiceId == invoiceId)
                .ToListAsync();
            return rows.Select(r => _mapper.Map<InvoiceItemModel>(r));
        }

        public async Task DeleteAsync(long id)
        {
            var row = await _context.InvoiceItems.FindAsync(id);
            if (row != null)
            {
                _context.InvoiceItems.Remove(row);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByInvoiceAsync(long invoiceId)
        {
            var rows = await _context.InvoiceItems
                .Where(x => x.InvoiceId == invoiceId)
                .ToListAsync();
            _context.InvoiceItems.RemoveRange(rows);
            await _context.SaveChangesAsync();
        }
    }
}
