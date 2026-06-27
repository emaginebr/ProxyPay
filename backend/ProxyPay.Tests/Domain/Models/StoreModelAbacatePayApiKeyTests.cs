using System;
using ProxyPay.Domain.Models;

namespace ProxyPay.Tests.Domain.Models;

// Feature: 002-store-abacatepay-apikey — Foundational (T004) / US1 (FR-004, FR-008)
public class StoreModelAbacatePayApiKeyTests
{
    [Fact]
    public void SetAbacatePayApiKey_WithValue_TrimsAndStores()
    {
        var store = new StoreModel();

        store.SetAbacatePayApiKey("  abc_live_123  ");

        Assert.Equal("abc_live_123", store.AbacatePayApiKey);
    }

    [Fact]
    public void SetAbacatePayApiKey_ReplacesPreviousValue()
    {
        var store = new StoreModel();
        store.SetAbacatePayApiKey("old_key");

        store.SetAbacatePayApiKey("new_key");

        Assert.Equal("new_key", store.AbacatePayApiKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetAbacatePayApiKey_EmptyOrWhitespace_Throws(string value)
    {
        var store = new StoreModel();
        store.SetAbacatePayApiKey("existing_key");

        Assert.Throws<ArgumentException>(() => store.SetAbacatePayApiKey(value));
        // valor anterior preservado (FR-008)
        Assert.Equal("existing_key", store.AbacatePayApiKey);
    }
}
