namespace ProxyPay.DTO.AbacatePay
{
    public class AbacatePayWebhookPayload
    {
        public string? Event { get; set; }
        public bool DevMode { get; set; }
        public AbacatePayWebhookData? Data { get; set; }
    }

    public class AbacatePayWebhookData
    {
        public string? Id { get; set; }
        public int Amount { get; set; }
        public string? Status { get; set; }
        public string? Url { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
    }
}
