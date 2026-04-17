using System;
using FluentValidation;
using ProxyPay.Domain.Validators;
using ProxyPay.Infra.Interfaces;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Infra;
using ProxyPay.Infra.Context;
using ProxyPay.Infra.Repository;
using ProxyPay.Domain.Core;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Services;
using ProxyPay.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ProxyPay.Domain;
using ProxyPay.Infra.Interfaces.AppServices;
using ProxyPay.Infra.AppServices;
using NAuth.ACL;
using NAuth.ACL.Interfaces;
using zTools.ACL.Interfaces;
using zTools.ACL;
using ProxyPay.Infra.Mappers;

namespace ProxyPay.Application
{
    public static class Startup
    {
        private static void injectDependency(Type serviceType, Type implementationType, IServiceCollection services, bool scoped = true)
        {
            if (scoped)
                services.AddScoped(serviceType, implementationType);
            else
                services.AddTransient(serviceType, implementationType);
        }

        public static void ConfigureProxyPay(this IServiceCollection services, bool scoped = true)
        {
            #region Tenant
            services.AddHttpContextAccessor();
            services.AddSingleton<ITenantCatalog, TenantCatalog>();
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddScoped<ITenantResolver, TenantResolver>();
            services.AddScoped<TenantDbContextFactory>();
            // Scoped: one DbContext per HTTP request, connection string resolved from the tenant of that request.
            // Must NOT be Singleton (cross-tenant bleed) nor Transient (multiple DbContexts per request fragments transactions).
            services.AddScoped(sp => sp.GetRequiredService<TenantDbContextFactory>().CreateDbContext());
            #endregion

            #region AutoMapper
            services.AddAutoMapper(typeof(InvoiceProfile), typeof(TransactionProfile), typeof(CustomerProfile), typeof(StoreProfile), typeof(BillingProfile));
            #endregion

            #region Validators
            services.AddValidatorsFromAssemblyContaining<QRCodeRequestValidator>(ServiceLifetime.Scoped);
            #endregion

            #region Infra
            injectDependency(typeof(IUnitOfWork), typeof(UnitOfWork), services, scoped);
            injectDependency(typeof(ILogCore), typeof(LogCore), services, scoped);
            #endregion

            #region Repository
            injectDependency(typeof(IInvoiceRepository<InvoiceModel>), typeof(InvoiceRepository), services, scoped);
            injectDependency(typeof(IInvoiceItemRepository<InvoiceItemModel>), typeof(InvoiceItemRepository), services, scoped);
            injectDependency(typeof(ITransactionRepository<TransactionModel>), typeof(TransactionRepository), services, scoped);
            injectDependency(typeof(ICustomerRepository<CustomerModel>), typeof(CustomerRepository), services, scoped);
            injectDependency(typeof(IStoreRepository<StoreModel>), typeof(StoreRepository), services, scoped);
            injectDependency(typeof(IBillingRepository<BillingModel>), typeof(BillingRepository), services, scoped);
            injectDependency(typeof(IBillingItemRepository<BillingItemModel>), typeof(BillingItemRepository), services, scoped);
            #endregion

            #region Client
            services.AddHttpClient();
            injectDependency(typeof(IUserClient), typeof(UserClient), services, scoped);
            injectDependency(typeof(IChatGPTClient), typeof(ChatGPTClient), services, scoped);
            injectDependency(typeof(IMailClient), typeof(MailClient), services, scoped);
            injectDependency(typeof(IFileClient), typeof(FileClient), services, scoped);
            injectDependency(typeof(IStringClient), typeof(StringClient), services, scoped);
            injectDependency(typeof(IDocumentClient), typeof(DocumentClient), services, scoped);
            #endregion

            #region AppService
            injectDependency(typeof(IAbacatePayAppService), typeof(AbacatePayAppService), services, scoped);
            #endregion

            #region Service
            injectDependency(typeof(IInvoiceService), typeof(InvoiceService), services, scoped);
            injectDependency(typeof(ITransactionService), typeof(TransactionService), services, scoped);
            injectDependency(typeof(ICustomerService), typeof(CustomerService), services, scoped);
            injectDependency(typeof(IStoreService), typeof(StoreService), services, scoped);
            injectDependency(typeof(IBillingService), typeof(BillingService), services, scoped);
            #endregion

            services.AddScoped<ITenantSecretProvider, NAuthTenantSecretProvider>();
            services.AddNAuth<NAuthTenantProvider>();
            services.AddNAuthAuthentication("BasicAuthentication");
        }
    }
}
