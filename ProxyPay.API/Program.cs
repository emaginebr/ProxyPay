using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProxyPay.Domain.Interfaces;
using Serilog;
using Serilog.Events;

namespace ProxyPay.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("HotChocolate", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("Starting ProxyPay API");

                var host = CreateHostBuilder(args).Build();

                EnforceTenantIsolationInvariants(host.Services);

                var env = host.Services.GetService<IWebHostEnvironment>();
                if (env?.EnvironmentName == "Docker")
                {
                    var config = host.Services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
                    var jwtSecret = config?["NAuth:JwtSecret"];
                    Log.Information("JWT_SECRET (NAuth): {JwtSecret}", jwtSecret ?? "(empty)");

                    var tenants = config?.GetSection("Tenants").GetChildren();
                    if (tenants != null)
                    {
                        foreach (var tenant in tenants)
                        {
                            var tenantJwt = tenant["JwtSecret"];
                            Log.Information("JWT_SECRET (Tenant {Tenant}): {JwtSecret}", tenant.Key, tenantJwt ?? "(empty)");
                        }
                    }
                }

                host.Run();
            }
            catch (System.Exception ex)
            {
                Log.Fatal(ex, "ProxyPay API terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void EnforceTenantIsolationInvariants(System.IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var catalog = scope.ServiceProvider.GetService<ITenantCatalog>();
            var configuration = scope.ServiceProvider.GetService<IConfiguration>();

            if (catalog == null || configuration == null)
                return;

            var seen = new Dictionary<string, string>(System.StringComparer.Ordinal);
            foreach (var tenantId in catalog.GetActiveTenantIds())
            {
                var cs = configuration[$"Tenants:{tenantId}:ConnectionString"];
                if (string.IsNullOrWhiteSpace(cs))
                    continue;

                if (seen.TryGetValue(cs, out var otherTenant))
                {
                    throw new System.InvalidOperationException(
                        $"Tenant isolation invariant violated: tenants '{otherTenant}' and '{tenantId}' share the same ConnectionString. " +
                        $"Each tenant MUST point to a physically distinct database (see .specify/memory/constitution.md §V and data-model.md invariant #1).");
                }

                seen[cs] = tenantId;
            }
        }
    }
}
