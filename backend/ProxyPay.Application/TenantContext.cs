using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using ProxyPay.Domain.Interfaces;

namespace ProxyPay.Application;

public class TenantContext : ITenantContext
{
    private static readonly AsyncLocal<string> _ambientTenantId = new AsyncLocal<string>();

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string TenantId
    {
        get
        {
            var ambient = _ambientTenantId.Value;
            if (!string.IsNullOrWhiteSpace(ambient))
                return ambient;

            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return null;

            if (context.Items.TryGetValue("TenantId", out var tenantObj)
                && tenantObj is string tenantId
                && !string.IsNullOrWhiteSpace(tenantId))
                return tenantId;

            return null;
        }
    }

    public static IDisposable EnterScope(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("tenantId must not be null or whitespace.", nameof(tenantId));

        var previous = _ambientTenantId.Value;
        _ambientTenantId.Value = tenantId;
        return new Scope(previous);
    }

    private sealed class Scope : IDisposable
    {
        private readonly string _previous;
        private bool _disposed;

        public Scope(string previous)
        {
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _ambientTenantId.Value = _previous;
            _disposed = true;
        }
    }
}
