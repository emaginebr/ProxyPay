using System.Net;
using FluentAssertions;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using ProxyPay.ApiTests.Fixtures;
using ProxyPay.ApiTests.Helpers;

namespace ProxyPay.ApiTests.Controllers;

/// <summary>
/// Testes do WebhookController (/webhook/abacatepay) — anônimo, protegido por
/// query "secret". US3 (P3). Por design (constituição §6 Padrão 3) o endpoint
/// sempre retorna 200, processando o evento apenas com segredo + payload válidos.
/// </summary>
[Collection("ApiTests")]
public class WebhookControllerTests
{
    private readonly ApiTestFixture _fixture;
    private readonly string _secret;

    public WebhookControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
        _secret = config["Webhook:Secret"] ?? string.Empty;
    }

    [Fact]
    public async Task Webhook_WithoutSecret_ShouldReturn200AndIgnore()
    {
        var response = await _fixture.CreateAnonymousRequest("webhook/abacatepay")
            .AllowAnyHttpStatus()
            .PostJsonAsync(TestDataHelper.CreateAbacatePayWebhookPayload());

        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task Webhook_WithInvalidSecret_ShouldReturn200AndIgnore()
    {
        var response = await _fixture.CreateAnonymousRequest("webhook/abacatepay")
            .SetQueryParam("secret", "segredo-invalido")
            .AllowAnyHttpStatus()
            .PostJsonAsync(TestDataHelper.CreateAbacatePayWebhookPayload());

        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task Webhook_WithValidSecretMissingData_ShouldReturn200()
    {
        var response = await _fixture.CreateAnonymousRequest("webhook/abacatepay")
            .SetQueryParam("secret", _secret)
            .AllowAnyHttpStatus()
            .PostJsonAsync(TestDataHelper.CreateAbacatePayWebhookPayload(withData: false));

        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task Webhook_WithValidSecretAndPayload_ShouldReturn200()
    {
        var response = await _fixture.CreateAnonymousRequest("webhook/abacatepay")
            .SetQueryParam("secret", _secret)
            .AllowAnyHttpStatus()
            .PostJsonAsync(TestDataHelper.CreateAbacatePayWebhookPayload());

        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
}
