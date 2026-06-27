namespace ProxyPay.DTO.GraphQL
{
    public class BalanceSummary
    {
        public double Balance { get; set; }
        public double TotalCredits { get; set; }
        public double TotalDebits { get; set; }
        public int TransactionCount { get; set; }
    }
}
