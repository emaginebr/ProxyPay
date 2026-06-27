using System.Collections.Generic;

namespace ProxyPay.Domain.Interfaces;

public interface ITenantCatalog
{
    IEnumerable<string> GetActiveTenantIds();
    bool IsKnownTenant(string tenantId);
}
