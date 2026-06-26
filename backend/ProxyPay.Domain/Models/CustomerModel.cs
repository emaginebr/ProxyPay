using System;

namespace ProxyPay.Domain.Models
{
    public class CustomerModel
    {
        public long CustomerId { get; set; }
        public long? StoreId { get; set; }
        public string Name { get; set; }
        public string DocumentId { get; set; }
        public string Cellphone { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public void SetStore(long storeId)
        {
            StoreId = storeId;
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            Name = name;
            MarkUpdated();
        }

        public void UpdateEmail(string email)
        {
            Email = email;
            MarkUpdated();
        }

        public void UpdateDocumentId(string documentId)
        {
            DocumentId = documentId;
            MarkUpdated();
        }

        public void UpdateCellphone(string cellphone)
        {
            Cellphone = cellphone;
            MarkUpdated();
        }

        public void UpdateContact(string name, string email, string documentId, string cellphone)
        {
            Name = name;
            Email = email;
            DocumentId = documentId;
            Cellphone = cellphone;
            MarkUpdated();
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
