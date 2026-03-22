using Ganesha.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Ganesha.Application
{
    public class TenantResolver : ITenantResolver
    {
        private readonly IConfiguration _configuration;
        private readonly ITenantContext _tenantContext;

        public TenantResolver(IConfiguration configuration, ITenantContext tenantContext)
        {
            _configuration = configuration;
            _tenantContext = tenantContext;
        }

        public string TenantId
        {
            get
            {
                var tenantId = _tenantContext.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                    tenantId = _configuration["Tenant:DefaultTenantId"];
                if (string.IsNullOrEmpty(tenantId))
                    throw new System.InvalidOperationException(
                        "Tenant:DefaultTenantId is not configured in appsettings.json.");
                return tenantId;
            }
        }

        public string ConnectionString
        {
            get
            {
                var cs = _configuration[$"Tenants:{TenantId}:ConnectionString"];
                if (string.IsNullOrEmpty(cs))
                    throw new System.InvalidOperationException(
                        $"ConnectionString not found for tenant '{TenantId}'. " +
                        $"Expected key: Tenants:{TenantId}:ConnectionString");
                return cs;
            }
        }

        public string JwtSecret
        {
            get
            {
                var secret = _configuration[$"Tenants:{TenantId}:JwtSecret"];
                if (string.IsNullOrEmpty(secret))
                    throw new System.InvalidOperationException(
                        $"JwtSecret not found for tenant '{TenantId}'. " +
                        $"Expected key: Tenants:{TenantId}:JwtSecret");
                return secret;
            }
        }

        public string BucketName
        {
            get
            {
                var bucket = _configuration[$"Tenants:{TenantId}:BucketName"];
                if (string.IsNullOrEmpty(bucket))
                    throw new System.InvalidOperationException(
                        $"BucketName not found for tenant '{TenantId}'. " +
                        $"Expected key: Tenants:{TenantId}:BucketName");
                return bucket;
            }
        }
    }
}
