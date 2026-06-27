namespace ProxyPay.DTO.AbacatePay
{
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
