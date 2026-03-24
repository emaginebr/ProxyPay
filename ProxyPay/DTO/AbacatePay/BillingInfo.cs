using System.Collections.Generic;

namespace ProxyPay.DTO.AbacatePay
{
    public class BillingInfo
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; }
        public bool DevMode { get; set; }
        public List<string> Methods { get; set; }
        public List<BillingProductInfo> Products { get; set; }
        public string Frequency { get; set; }
        public string NextBilling { get; set; }
        public BillingCustomerInfo Customer { get; set; }
        public bool AllowCoupons { get; set; }
        public List<string> Coupons { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
}
