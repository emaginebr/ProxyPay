namespace ProxyPay.DTO.AbacatePay
{
    public class PixQrCodeInfo
    {
        public string Id { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; }
        public bool DevMode { get; set; }
        public string BrCode { get; set; }
        public string BrCodeBase64 { get; set; }
        public int PlatformFee { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string ExpiresAt { get; set; }
        public object Metadata { get; set; }
    }
}
