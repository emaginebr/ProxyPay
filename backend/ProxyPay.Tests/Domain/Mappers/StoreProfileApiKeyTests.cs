using AutoMapper;
using ProxyPay.Domain.Models;
using ProxyPay.DTO.Billing;
using ProxyPay.DTO.Store;
using ProxyPay.Infra.Mappers;

namespace ProxyPay.Tests.Domain.Mappers;

// Feature: 002-store-abacatepay-apikey — Foundational (T005) / US1 (FR-007, FR-008)
public class StoreProfileApiKeyTests
{
    private static IMapper BuildMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<StoreProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public void StoreUpdateInfo_To_StoreModel_DoesNotOverwriteApiKey()
    {
        var mapper = BuildMapper();
        var existing = new StoreModel
        {
            StoreId = 1,
            Name = "Original",
            AbacatePayApiKey = "secret_key"
        };
        var update = new StoreUpdateInfo
        {
            StoreId = 1,
            Name = "Updated",
            Email = "store@example.com",
            BillingStrategy = BillingStrategyEnum.FirstDayOfMonth
        };

        mapper.Map(update, existing);

        Assert.Equal("Updated", existing.Name);
        // A credencial NÃO pode ser apagada pelo update geral (FR-007/FR-008)
        Assert.Equal("secret_key", existing.AbacatePayApiKey);
    }
}
