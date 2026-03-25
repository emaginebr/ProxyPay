using AutoMapper;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository<InvoiceModel> _invoiceRepository;
        private readonly IInvoiceItemRepository<InvoiceItemModel> _invoiceItemRepository;
        private readonly IMapper _mapper;

        public InvoiceService(
            IInvoiceRepository<InvoiceModel> invoiceRepository,
            IInvoiceItemRepository<InvoiceItemModel> invoiceItemRepository,
            IMapper mapper
        )
        {
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _mapper = mapper;
        }

        public async Task<InvoiceModel> GetByIdAsync(long invoiceId)
        {
            return await _invoiceRepository.GetByIdAsync(invoiceId);
        }

        public async Task<InvoiceInfo> GetInvoiceInfoAsync(InvoiceModel model)
        {
            var info = _mapper.Map<InvoiceInfo>(model);
            var items = await _invoiceItemRepository.ListByInvoiceAsync(model.InvoiceId);
            info.Items = items.Select(i => _mapper.Map<InvoiceItemInfo>(i)).ToList();
            return info;
        }

        public async Task<IList<InvoiceInfo>> ListByStoreAsync(long storeId)
        {
            var invoices = await _invoiceRepository.ListByStoreAsync(storeId);
            var result = new List<InvoiceInfo>();
            foreach (var invoice in invoices)
            {
                result.Add(await GetInvoiceInfoAsync(invoice));
            }
            return result;
        }

        private double CalculateItemTotal(int quantity, double unitPrice, double discount)
        {
            return (quantity * unitPrice) - discount;
        }

        public async Task<InvoiceModel> InsertAsync(InvoiceInsertInfo invoice, long storeId)
        {
            if (invoice.Items == null || !invoice.Items.Any())
                throw new Exception("Invoice must have at least one item");

            var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(storeId);

            var model = _mapper.Map<InvoiceModel>(invoice);
            model.StoreId = storeId;
            model.InvoiceNumber = invoiceNumber;
            model.Status = InvoiceStatusEnum.Draft;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            var savedInvoice = await _invoiceRepository.InsertAsync(model);

            foreach (var item in invoice.Items)
            {
                var itemModel = _mapper.Map<InvoiceItemModel>(item);
                itemModel.InvoiceId = savedInvoice.InvoiceId;
                itemModel.Total = CalculateItemTotal(item.Quantity, item.UnitPrice, item.Discount);
                itemModel.CreatedAt = DateTime.Now;
                await _invoiceItemRepository.InsertAsync(itemModel);
            }

            return savedInvoice;
        }

        public async Task<InvoiceModel> UpdateAsync(InvoiceUpdateInfo invoice)
        {
            var existing = await _invoiceRepository.GetByIdAsync(invoice.InvoiceId);
            if (existing == null)
                throw new Exception("Invoice not found");

            if (invoice.Items == null || !invoice.Items.Any())
                throw new Exception("Invoice must have at least one item");

            existing.Notes = invoice.Notes;
            existing.Status = invoice.Status;
            existing.Discount = invoice.Discount;
            existing.DueDate = invoice.DueDate;
            existing.UpdatedAt = DateTime.Now;

            if (invoice.Status == InvoiceStatusEnum.Paid && existing.PaidAt == null)
                existing.PaidAt = DateTime.Now;

            var updated = await _invoiceRepository.UpdateAsync(existing);

            await _invoiceItemRepository.DeleteByInvoiceAsync(existing.InvoiceId);

            foreach (var item in invoice.Items)
            {
                var itemModel = _mapper.Map<InvoiceItemModel>(item);
                itemModel.InvoiceId = existing.InvoiceId;
                itemModel.Total = CalculateItemTotal(item.Quantity, item.UnitPrice, item.Discount);
                itemModel.CreatedAt = DateTime.Now;
                await _invoiceItemRepository.InsertAsync(itemModel);
            }

            return updated;
        }

        public async Task DeleteAsync(long invoiceId)
        {
            var existing = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (existing == null)
                throw new Exception("Invoice not found");

            await _invoiceItemRepository.DeleteByInvoiceAsync(invoiceId);
            await _invoiceRepository.DeleteAsync(invoiceId);
        }
    }
}
