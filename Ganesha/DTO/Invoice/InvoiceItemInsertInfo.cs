using System.Text.Json.Serialization;

namespace Ganesha.DTO.Invoice
{
    public class InvoiceItemInsertInfo
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("unitPrice")]
        public double UnitPrice { get; set; }
        [JsonPropertyName("discount")]
        public double Discount { get; set; }
    }
}
