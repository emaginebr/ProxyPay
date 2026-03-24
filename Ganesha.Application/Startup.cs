using System;
using Ganesha.Infra.Interfaces;
using Ganesha.Infra.Interfaces.Repository;
using Ganesha.Infra;
using Ganesha.Infra.Context;
using Ganesha.Infra.Repository;
using Ganesha.Domain.Core;
using Ganesha.Domain.Models;
using Ganesha.Domain.Services;
using Ganesha.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ganesha.ACL;
using Ganesha.ACL.Handlers;
using Ganesha.ACL.Interfaces;
using Ganesha.Domain;
using Microsoft.Extensions.Configuration;
using NAuth.ACL;
using NAuth.ACL.Interfaces;
using zTools.ACL.Interfaces;
using zTools.ACL;

namespace Ganesha.Application
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

        public static void ConfigureGanesha(this IServiceCollection services, bool scoped = true)
        {
            #region Tenant
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddScoped<ITenantResolver, TenantResolver>();
            services.AddScoped<TenantDbContextFactory>();
            services.AddScoped(sp => sp.GetRequiredService<TenantDbContextFactory>().CreateDbContext());
            services.AddTransient<TenantHeaderHandler>();
            #endregion

            #region Infra
            injectDependency(typeof(IUnitOfWork), typeof(UnitOfWork), services, scoped);
            injectDependency(typeof(ILogCore), typeof(LogCore), services, scoped);
            #endregion

            #region Repository
            injectDependency(typeof(IInvoiceRepository<InvoiceModel>), typeof(InvoiceRepository), services, scoped);
            injectDependency(typeof(IInvoiceItemRepository<InvoiceItemModel>), typeof(InvoiceItemRepository), services, scoped);
            injectDependency(typeof(ITransactionRepository<TransactionModel>), typeof(TransactionRepository), services, scoped);
            #endregion

            #region Client
            services.AddHttpClient();
            injectDependency(typeof(IUserClient), typeof(UserClient), services, scoped);
            injectDependency(typeof(IChatGPTClient), typeof(ChatGPTClient), services, scoped);
            injectDependency(typeof(IMailClient), typeof(MailClient), services, scoped);
            injectDependency(typeof(IFileClient), typeof(FileClient), services, scoped);
            injectDependency(typeof(IStringClient), typeof(StringClient), services, scoped);
            injectDependency(typeof(IDocumentClient), typeof(DocumentClient), services, scoped);
            injectDependency(typeof(IInvoiceClient), typeof(InvoiceClient), services, scoped);
            injectDependency(typeof(ITransactionClient), typeof(TransactionClient), services, scoped);
            #endregion

            #region Service
            injectDependency(typeof(IInvoiceService), typeof(InvoiceService), services, scoped);
            injectDependency(typeof(ITransactionService), typeof(TransactionService), services, scoped);
            #endregion

            services.AddScoped<ITenantSecretProvider, NAuthTenantSecretProvider>();
            services.AddNAuth<NAuthTenantProvider>();
            services.AddNAuthAuthentication("BasicAuthentication");
        }
    }
}
