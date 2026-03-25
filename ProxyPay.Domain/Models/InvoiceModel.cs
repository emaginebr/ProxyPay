using ProxyPay.DTO.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyPay.Domain.Models
{
    public class InvoiceModel
    {
        public long InvoiceId { get; set; }
        public long? CustomerId { get; set; }
        public long? StoreId { get; set; }
        public string InvoiceNumber { get; set; }
        public string Notes { get; set; }
        public InvoiceStatusEnum Status { get; set; }
        public double Discount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IList<InvoiceItemModel> Items { get; set; } = new List<InvoiceItemModel>();

        public void SetStore(long storeId)
        {
            StoreId = storeId;
        }

        public void SetCustomer(long customerId)
        {
            CustomerId = customerId;
        }

        public void SetInvoiceNumber(string invoiceNumber)
        {
            InvoiceNumber = invoiceNumber;
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes;
            MarkUpdated();
        }

        public void UpdateDiscount(double discount)
        {
            Discount = discount;
            MarkUpdated();
        }

        public void UpdateDueDate(DateTime dueDate)
        {
            DueDate = dueDate;
            MarkUpdated();
        }

        public void MarkAsDraft()
        {
            Status = InvoiceStatusEnum.Draft;
            MarkUpdated();
        }

        public void MarkAsSent()
        {
            Status = InvoiceStatusEnum.Sent;
            MarkUpdated();
        }

        public void MarkAsPaid()
        {
            Status = InvoiceStatusEnum.Paid;
            if (PaidAt == null)
                PaidAt = DateTime.Now;
            MarkUpdated();
        }

        public void MarkAsOverdue()
        {
            Status = InvoiceStatusEnum.Overdue;
            MarkUpdated();
        }

        public void Cancel()
        {
            Status = InvoiceStatusEnum.Cancelled;
            MarkUpdated();
        }

        public void SetStatus(InvoiceStatusEnum status)
        {
            if (status == InvoiceStatusEnum.Paid)
            {
                MarkAsPaid();
                return;
            }
            Status = status;
            MarkUpdated();
        }

        public void AddItem(InvoiceItemModel item)
        {
            item.MarkCreated();
            Items.Add(item);
        }

        public void RemoveItem(long invoiceItemId)
        {
            var item = Items.FirstOrDefault(i => i.InvoiceItemId == invoiceItemId);
            if (item != null)
                Items.Remove(item);
        }

        public void ClearItems()
        {
            Items.Clear();
        }

        public double GetSubtotal()
        {
            return Items.Sum(i => i.Total);
        }

        public double GetTotal()
        {
            return GetSubtotal() - Discount;
        }

        public bool IsOverdue()
        {
            return Status != InvoiceStatusEnum.Paid
                && Status != InvoiceStatusEnum.Cancelled
                && DueDate < DateTime.Now;
        }

        public void MarkCreated()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public void MarkUpdated()
        {
            UpdatedAt = DateTime.Now;
        }
    }
}
