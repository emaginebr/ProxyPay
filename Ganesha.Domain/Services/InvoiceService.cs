using Ganesha.Infra.Interfaces.Repository;
using Ganesha.Domain.Mappers;
using Ganesha.Domain.Models;
using Ganesha.Domain.Interfaces;
using Ganesha.DTO.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ganesha.Domain.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository<InvoiceModel> _invoiceRepository;
        private readonly IInvoiceItemRepository<InvoiceItemModel> _invoiceItemRepository;

        public InvoiceService(
            IInvoiceRepository<InvoiceModel> invoiceRepository,
            IInvoiceItemRepository<InvoiceItemModel> invoiceItemRepository
        )
        {
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
        }

        public async Task<InvoiceModel> GetByIdAsync(long invoiceId, long userId)
        {
            var model = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (model == null)
                return null;

            if (model.UserId != userId)
                throw new UnauthorizedAccessException("Access denied: invoice does not belong to this user");

            return model;
        }

        public async Task<InvoiceInfo> GetInvoiceInfoAsync(InvoiceModel model)
        {
            var info = InvoiceMapper.ToInfo(model);
            var items = await _invoiceItemRepository.ListByInvoiceAsync(model.InvoiceId);
            info.Items = items.Select(InvoiceItemMapper.ToInfo).ToList();
            return info;
        }

        public async Task<IList<InvoiceInfo>> ListByUserAsync(long userId)
        {
            var invoices = await _invoiceRepository.ListByUserAsync(userId);
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

        public async Task<InvoiceModel> InsertAsync(InvoiceInsertInfo invoice, long userId)
        {
            if (invoice.Items == null || !invoice.Items.Any())
                throw new Exception("Invoice must have at least one item");

            var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(userId);

            double subTotal = 0;
            foreach (var item in invoice.Items)
            {
                subTotal += CalculateItemTotal(item.Quantity, item.UnitPrice, item.Discount);
            }

            var model = new InvoiceModel
            {
                UserId = userId,
                InvoiceNumber = invoiceNumber,
                Notes = invoice.Notes,
                Status = InvoiceStatusEnum.Draft,
                SubTotal = subTotal,
                Discount = invoice.Discount,
                Tax = invoice.Tax,
                Total = subTotal - invoice.Discount + invoice.Tax,
                DueDate = invoice.DueDate,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var savedInvoice = await _invoiceRepository.InsertAsync(model);

            foreach (var item in invoice.Items)
            {
                var itemModel = new InvoiceItemModel
                {
                    InvoiceId = savedInvoice.InvoiceId,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    Total = CalculateItemTotal(item.Quantity, item.UnitPrice, item.Discount),
                    CreatedAt = DateTime.Now
                };
                await _invoiceItemRepository.InsertAsync(itemModel);
            }

            return savedInvoice;
        }

        public async Task<InvoiceModel> UpdateAsync(InvoiceUpdateInfo invoice, long userId)
        {
            var existing = await _invoiceRepository.GetByIdAsync(invoice.InvoiceId);
            if (existing == null)
                throw new Exception("Invoice not found");

            if (existing.UserId != userId)
                throw new UnauthorizedAccessException("Access denied: invoice does not belong to this user");

            if (invoice.Items == null || !invoice.Items.Any())
                throw new Exception("Invoice must have at least one item");

            double subTotal = 0;
            foreach (var item in invoice.Items)
            {
                subTotal += CalculateItemTotal(item.Quantity, item.UnitPrice, item.Discount);
            }

            existing.Notes = invoice.Notes;
            existing.Status = invoice.Status;
            existing.SubTotal = subTotal;
            existing.Discount = invoice.Discount;
            existing.Tax = invoice.Tax;
            existing.Total = subTotal - invoice.Discount + invoice.Tax;
            existing.DueDate = invoice.DueDate;
            existing.UpdatedAt = DateTime.Now;

            if (invoice.Status == InvoiceStatusEnum.Paid && existing.PaidAt == null)
                existing.PaidAt = DateTime.Now;

            var updated = await _invoiceRepository.UpdateAsync(existing);

            await _invoiceItemRepository.DeleteByInvoiceAsync(existing.InvoiceId);

            foreach (var item in invoice.Items)
            {
                var itemModel = new InvoiceItemModel
                {
                    InvoiceId = existing.InvoiceId,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    Total = CalculateItemTotal(item.Quantity, item.UnitPrice, item.Discount),
                    CreatedAt = DateTime.Now
                };
                await _invoiceItemRepository.InsertAsync(itemModel);
            }

            return updated;
        }

        public async Task DeleteAsync(long invoiceId, long userId)
        {
            var existing = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (existing == null)
                throw new Exception("Invoice not found");

            if (existing.UserId != userId)
                throw new UnauthorizedAccessException("Access denied: invoice does not belong to this user");

            await _invoiceItemRepository.DeleteByInvoiceAsync(invoiceId);
            await _invoiceRepository.DeleteAsync(invoiceId);
        }
    }
}
