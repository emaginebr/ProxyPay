using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace ProxyPay.DTO.AbacatePay
{
    public class BillingCreateRequest
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AbacatePayBillingFrequency Frequency { get; set; } = AbacatePayBillingFrequency.ONE_TIME;

        public List<string> Methods { get; set; } = new List<string> { "PIX" };
        public List<BillingProductRequest> Products { get; set; }
        public string ReturnUrl { get; set; }
        public string CompletionUrl { get; set; }
        public string CustomerId { get; set; }
        public AbacatePayCustomerRequest Customer { get; set; }
        public bool AllowCoupons { get; set; }
        public List<string> Coupons { get; set; }
        public string ExternalId { get; set; }
        public object Metadata { get; set; }
    }
}
