using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ganesha.API.Middlewares
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId)
                && !string.IsNullOrWhiteSpace(tenantId))
            {
                context.Items["TenantId"] = tenantId.ToString();
            }

            await _next(context);
        }
    }
}
