using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NAuth.ACL.Interfaces;

namespace ProxyPay.Application
{
    public class NAuthTenantSecretProvider : ITenantSecretProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NAuthTenantSecretProvider> _logger;

        public NAuthTenantSecretProvider(IConfiguration configuration, ILogger<NAuthTenantSecretProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string? GetJwtSecret(string tenantId)
        {
            var secret = _configuration[$"Tenants:{tenantId}:JwtSecret"];
            _logger.LogInformation("JWT_SECRET for tenant '{TenantId}': {Secret}",
                tenantId,
                string.IsNullOrWhiteSpace(secret) ? "(empty)" : secret);
            return secret;
        }
    }
}
