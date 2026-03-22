using Ganesha.Infra.Interfaces.Repository;
using Ganesha.Infra.Context;
using Ganesha.Infra.Mappers;
using Ganesha.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ganesha.Infra.Repository
{
    public class InvoiceItemRepository : IInvoiceItemRepository<InvoiceItemModel>
    {
        private readonly GaneshaContext _context;

        public InvoiceItemRepository(GaneshaContext context)
        {
            _context = context;
        }

        public async Task<InvoiceItemModel> InsertAsync(InvoiceItemModel model)
        {
            var row = new InvoiceItem();
            InvoiceItemDbMapper.ToEntity(model, row);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.InvoiceItemId = row.InvoiceItemId;
            return model;
        }

        public async Task<InvoiceItemModel> UpdateAsync(InvoiceItemModel model)
        {
            var row = await _context.InvoiceItems.FindAsync(model.InvoiceItemId);
            InvoiceItemDbMapper.ToEntity(model, row);
            _context.InvoiceItems.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<InvoiceItemModel> GetByIdAsync(long id)
        {
            var row = await _context.InvoiceItems.FindAsync(id);
            if (row == null)
                return null;
            return InvoiceItemDbMapper.ToModel(row);
        }

        public async Task<IEnumerable<InvoiceItemModel>> ListByInvoiceAsync(long invoiceId)
        {
            var rows = await _context.InvoiceItems
                .Where(x => x.InvoiceId == invoiceId)
                .ToListAsync();
            return rows.Select(InvoiceItemDbMapper.ToModel);
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
