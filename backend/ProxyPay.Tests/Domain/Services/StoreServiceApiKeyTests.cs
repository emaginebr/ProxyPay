using System;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Services;
using ProxyPay.Infra.Interfaces.Repository;

namespace ProxyPay.Tests.Domain.Services;

// Feature: 002-store-abacatepay-apikey — US1 (FR-003, FR-004, FR-009)
public class StoreServiceApiKeyTests
{
    private static StoreService BuildService(Mock<IStoreRepository<StoreModel>> repo)
    {
        return new StoreService(repo.Object, new Mock<IMapper>().Object);
    }

    [Fact]
    public async Task UpdateAbacatePayApiKeyAsync_StoreNotFound_Throws()
    {
        var repo = new Mock<IStoreRepository<StoreModel>>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<long>())).ReturnsAsync((StoreModel)null);
        var service = BuildService(repo);

        await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateAbacatePayApiKeyAsync(1, "key", 10));

        repo.Verify(r => r.UpdateAsync(It.IsAny<StoreModel>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAbacatePayApiKeyAsync_NotOwner_ThrowsUnauthorized()
    {
        var store = new StoreModel { StoreId = 1, UserId = 99 };
        var repo = new Mock<IStoreRepository<StoreModel>>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(store);
        var service = BuildService(repo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateAbacatePayApiKeyAsync(1, "key", 10));

        repo.Verify(r => r.UpdateAsync(It.IsAny<StoreModel>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAbacatePayApiKeyAsync_Valid_SetsAndPersists()
    {
        var store = new StoreModel { StoreId = 1, UserId = 10 };
        var repo = new Mock<IStoreRepository<StoreModel>>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(store);
        repo.Setup(r => r.UpdateAsync(It.IsAny<StoreModel>())).ReturnsAsync(store);
        var service = BuildService(repo);

        await service.UpdateAbacatePayApiKeyAsync(1, "  new_key  ", 10);

        Assert.Equal("new_key", store.AbacatePayApiKey);
        repo.Verify(r => r.UpdateAsync(It.Is<StoreModel>(s => s.AbacatePayApiKey == "new_key")), Times.Once);
    }
}
