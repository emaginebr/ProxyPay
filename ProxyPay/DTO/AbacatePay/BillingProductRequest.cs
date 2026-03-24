namespace ProxyPay.DTO.AbacatePay
{
    public class BillingProductRequest
    {
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
