namespace ProxyPay.DTO.AbacatePay
{
    public class AbacatePayWebhookPayload
    {
        public string? Event { get; set; }
        public bool DevMode { get; set; }
        public AbacatePayWebhookData? Data { get; set; }
    }
}
