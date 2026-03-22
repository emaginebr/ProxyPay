using Ganesha.Domain.Interfaces;
using Ganesha.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Ganesha.Application
{
    public class TenantDbContextFactory
    {
        private readonly ITenantResolver _tenantResolver;

        public TenantDbContextFactory(ITenantResolver tenantResolver)
        {
            _tenantResolver = tenantResolver;
        }

        public GaneshaContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<GaneshaContext>();
            optionsBuilder.UseLazyLoadingProxies().UseNpgsql(_tenantResolver.ConnectionString);
            return new GaneshaContext(optionsBuilder.Options);
        }
    }
}
