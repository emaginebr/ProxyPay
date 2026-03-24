namespace ProxyPay.DTO.AbacatePay
{
    public class AbacatePayResponse<T>
    {
        public T Data { get; set; }
        public string Error { get; set; }
    }
}
