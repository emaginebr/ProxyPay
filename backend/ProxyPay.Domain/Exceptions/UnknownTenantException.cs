using System;

namespace ProxyPay.Domain.Exceptions;

public class UnknownTenantException : Exception
{
    public string TenantId { get; }

    public UnknownTenantException(string tenantId)
        : base($"Tenant '{tenantId}' is not registered in the catalog.")
    {
        TenantId = tenantId;
    }
}
