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
    public class InvoiceRepository : IInvoiceRepository<InvoiceModel>
    {
        private readonly ProxyPayContext _context;
        private readonly IMapper _mapper;

        public InvoiceRepository(ProxyPayContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<InvoiceModel> InsertAsync(InvoiceModel model)
        {
            var row = _mapper.Map<Invoice>(model);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.InvoiceId = row.InvoiceId;
            return model;
        }

        public async Task<InvoiceModel> UpdateAsync(InvoiceModel model)
        {
            var row = await _context.Invoices.FindAsync(model.InvoiceId);
            _mapper.Map(model, row);
            _context.Invoices.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<InvoiceModel> GetByIdAsync(long id)
        {
            var row = await _context.Invoices.FindAsync(id);
            if (row == null)
                return null;
            return _mapper.Map<InvoiceModel>(row);
        }

        public async Task<InvoiceModel> GetByNumberAsync(string invoiceNumber)
        {
            var row = await _context.Invoices
                .Where(x => x.InvoiceNumber == invoiceNumber)
                .FirstOrDefaultAsync();
            if (row == null)
                return null;
            return _mapper.Map<InvoiceModel>(row);
        }

        public async Task<IEnumerable<InvoiceModel>> ListByStoreAsync(long storeId)
        {
            var rows = await _context.Invoices
                .Where(x => x.StoreId == storeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return rows.Select(r => _mapper.Map<InvoiceModel>(r));
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

        public async Task<string> GenerateInvoiceNumberAsync(long storeId)
        {
            var count = await _context.Invoices
                .Where(x => x.StoreId == storeId)
                .CountAsync();
            return $"INV-{storeId:D4}-{(count + 1):D6}";
        }
    }
}
