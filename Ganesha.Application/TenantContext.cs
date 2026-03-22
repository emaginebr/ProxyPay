using Ganesha.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Ganesha.Application
{
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string TenantId
        {
            get
            {
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
    }
}
