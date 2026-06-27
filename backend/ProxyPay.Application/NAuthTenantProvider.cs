using NAuth.ACL.Interfaces;
using ProxyPay.Domain.Interfaces;

namespace ProxyPay.Application
{
    public class NAuthTenantProvider : ITenantProvider
    {
        private readonly ITenantContext _tenantContext;

        public NAuthTenantProvider(ITenantContext tenantContext)
        {
            _tenantContext = tenantContext;
        }

        public string? GetTenantId()
        {
            try
            {
                return _tenantContext.TenantId;
            }
            catch
            {
                return null;
            }
        }
    }
}
