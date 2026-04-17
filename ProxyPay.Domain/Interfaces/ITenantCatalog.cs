using System;
using System.Collections.Generic;

namespace ProxyPay.Domain.Interfaces;

public interface ITenantCatalog
{
    IEnumerable<string> GetActiveTenantIds();
    bool IsKnownTenant(string tenantId);
}

public class UnknownTenantException : Exception
{
    public string TenantId { get; }

    public UnknownTenantException(string tenantId)
        : base($"Tenant '{tenantId}' is not registered in the catalog.")
    {
        TenantId = tenantId;
    }
}
