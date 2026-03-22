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
    public class InvoiceRepository : IInvoiceRepository<InvoiceModel>
    {
        private readonly GaneshaContext _context;

        public InvoiceRepository(GaneshaContext context)
        {
            _context = context;
        }

        public async Task<InvoiceModel> InsertAsync(InvoiceModel model)
        {
            var row = new Invoice();
            InvoiceDbMapper.ToEntity(model, row);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.InvoiceId = row.InvoiceId;
            return model;
        }

        public async Task<InvoiceModel> UpdateAsync(InvoiceModel model)
        {
            var row = await _context.Invoices.FindAsync(model.InvoiceId);
            InvoiceDbMapper.ToEntity(model, row);
            _context.Invoices.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<InvoiceModel> GetByIdAsync(long id)
        {
            var row = await _context.Invoices.FindAsync(id);
            if (row == null)
                return null;
            return InvoiceDbMapper.ToModel(row);
        }

        public async Task<InvoiceModel> GetByNumberAsync(string invoiceNumber)
        {
            var row = await _context.Invoices
                .Where(x => x.InvoiceNumber == invoiceNumber)
                .FirstOrDefaultAsync();
            if (row == null)
                return null;
            return InvoiceDbMapper.ToModel(row);
        }

        public async Task<IEnumerable<InvoiceModel>> ListByUserAsync(long userId)
        {
            var rows = await _context.Invoices
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return rows.Select(InvoiceDbMapper.ToModel);
        }

        public async Task DeleteAsync(long id)
        {
            var row = await _context.Invoices.FindAsync(id);
            if (row != null)
            {
                _context.Invoices.Remove(row);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GenerateInvoiceNumberAsync(long userId)
        {
            var count = await _context.Invoices
                .Where(x => x.UserId == userId)
                .CountAsync();
            return $"INV-{userId:D4}-{(count + 1):D6}";
        }
    }
}
