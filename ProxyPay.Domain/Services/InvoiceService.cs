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
            var model = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (model == null)
                return null;

            var items = await _invoiceItemRepository.ListByInvoiceAsync(invoiceId);
            model.Items = items.ToList();

            return model;
        }

        public async Task<InvoiceInfo> GetInvoiceInfoAsync(InvoiceModel model)
        {
            var info = _mapper.Map<InvoiceInfo>(model);
            info.Items = model.Items.Select(i => _mapper.Map<InvoiceItemInfo>(i)).ToList();
            return info;
        }

        public async Task<InvoiceModel> InsertAsync(InvoiceInsertInfo invoice, long storeId, long customerId)
        {
            var model = await InsertAsync(invoice, storeId);
            model.SetCustomer(customerId);
            await _invoiceRepository.UpdateAsync(model);
            return model;
        }

        public async Task<InvoiceModel> InsertAsync(InvoiceInsertInfo invoice, long storeId)
        {
            if (invoice.Items == null || !invoice.Items.Any())
                throw new Exception("Invoice must have at least one item");

            var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(storeId);

            var model = _mapper.Map<InvoiceModel>(invoice);
            model.SetStore(storeId);
            model.SetInvoiceNumber(invoiceNumber);
            model.MarkAsDraft();
            model.MarkCreated();

            var savedInvoice = await _invoiceRepository.InsertAsync(model);

            foreach (var item in invoice.Items)
            {
                var itemModel = _mapper.Map<InvoiceItemModel>(item);
                itemModel.InvoiceId = savedInvoice.InvoiceId;
                itemModel.MarkCreated();
                var savedItem = await _invoiceItemRepository.InsertAsync(itemModel);
                savedInvoice.Items.Add(savedItem);
            }

            return savedInvoice;
        }

        public async Task<InvoiceModel> UpdateAsync(InvoiceUpdateInfo invoice)
        {
            var existing = await GetByIdAsync(invoice.InvoiceId);
            if (existing == null)
                throw new Exception("Invoice not found");

            if (invoice.Items == null || !invoice.Items.Any())
                throw new Exception("Invoice must have at least one item");

            existing.Notes = invoice.Notes;
            existing.Discount = invoice.Discount;
            existing.DueDate = invoice.DueDate;
            existing.SetStatus(invoice.Status);

            var updated = await _invoiceRepository.UpdateAsync(existing);

            await _invoiceItemRepository.DeleteByInvoiceAsync(existing.InvoiceId);
            updated.ClearItems();

            foreach (var item in invoice.Items)
            {
                var itemModel = _mapper.Map<InvoiceItemModel>(item);
                itemModel.InvoiceId = existing.InvoiceId;
                itemModel.MarkCreated();
                var savedItem = await _invoiceItemRepository.InsertAsync(itemModel);
                updated.Items.Add(savedItem);
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
