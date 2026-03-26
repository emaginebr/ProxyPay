using System.Collections.Generic;
using System.Text.Json.Serialization;
using ProxyPay.DTO.Customer;

namespace ProxyPay.DTO.Invoice
{
    public class QRCodeRequest
    {
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }
        [JsonPropertyName("customer")]
        public CustomerInsertInfo Customer { get; set; }
        [JsonPropertyName("items")]
        public IList<InvoiceItemRequest> Items { get; set; }
    }
}
