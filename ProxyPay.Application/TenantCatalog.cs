using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using ProxyPay.Domain.Interfaces;

namespace ProxyPay.Application;

public class TenantCatalog : ITenantCatalog
{
    private readonly IConfiguration _configuration;

    public TenantCatalog(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IEnumerable<string> GetActiveTenantIds()
    {
        var tenantsSection = _configuration.GetSection("Tenants");
        if (!tenantsSection.Exists())
            return Enumerable.Empty<string>();

        return tenantsSection
            .GetChildren()
            .Select(child => child.Key)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToList();
    }

    public bool IsKnownTenant(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return false;

        var tenantSection = _configuration.GetSection($"Tenants:{tenantId}");
        if (!tenantSection.Exists())
            return false;

        var connectionString = tenantSection["ConnectionString"];
        return !string.IsNullOrWhiteSpace(connectionString);
    }
}
