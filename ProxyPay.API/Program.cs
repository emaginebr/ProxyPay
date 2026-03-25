using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    }
}
