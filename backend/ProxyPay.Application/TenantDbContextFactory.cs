using ProxyPay.Domain.Interfaces;
using ProxyPay.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace ProxyPay.Application
{
    public class TenantDbContextFactory
    {
        private readonly ITenantResolver _tenantResolver;

        public TenantDbContextFactory(ITenantResolver tenantResolver)
        {
            _tenantResolver = tenantResolver;
        }

        public ProxyPayContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProxyPayContext>();
            optionsBuilder.UseLazyLoadingProxies().UseNpgsql(_tenantResolver.ConnectionString);
            return new ProxyPayContext(optionsBuilder.Options);
        }
    }
}
