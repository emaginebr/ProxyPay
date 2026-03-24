namespace ProxyPay.DTO.AbacatePay
{
    public class PixQrCodeCreateRequest
    {
        public int Amount { get; set; }
        public int? ExpiresIn { get; set; }
        public string Description { get; set; }
        public AbacatePayCustomerRequest Customer { get; set; }
        public object Metadata { get; set; }
    }
}
