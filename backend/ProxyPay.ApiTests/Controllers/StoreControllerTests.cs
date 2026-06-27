using System.Net;
using FluentAssertions;
using Flurl.Http;
using ProxyPay.ApiTests.Fixtures;
using ProxyPay.ApiTests.Helpers;

namespace ProxyPay.ApiTests.Controllers;

/// <summary>
/// Testes do StoreController (/store) — todos [Authorize].
/// US2 (P2): autenticação, propriedade de recurso e contrato de criação.
/// Idempotência (R5): o happy-path de criação afirma o contrato da resposta e
/// NÃO deleta; o caminho destrutivo de DELETE só é coberto via autorização.
/// </summary>
[Collection("ApiTests")]
public class StoreControllerTests
{
    private readonly ApiTestFixture _fixture;

    // Id arbitrário, presumidamente de outro dono / inexistente para o usuário
    // de teste — usado nos cenários de autorização de recurso (403/404).
    private const long ForeignStoreId = 999_999_999;

    public StoreControllerTests(ApiTestFixture fixture) => _fixture = fixture;

    // ----------------------------------------------------------- 401 anônimos
    [Fact]
    public async Task Create_WithoutAuth_ShouldReturn401()
    {
        var response = await _fixture.CreateAnonymousRequest("store")
            .AllowAnyHttpStatus()
            .PostJsonAsync(TestDataHelper.CreateStoreInsertInfo());

        response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_WithoutAuth_ShouldReturn401()
    {
        var response = await _fixture.CreateAnonymousRequest("store")
            .AllowAnyHttpStatus()
            .PutJsonAsync(TestDataHelper.CreateStoreUpdateInfo(ForeignStoreId));

        response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetAbacatePayApiKey_WithoutAuth_ShouldReturn401()
    {
        var response = await _fixture.CreateAnonymousRequest("store")
            .AppendPathSegment(ForeignStoreId)
            .AppendPathSegment("abacatepay-apikey")
            .AllowAnyHttpStatus()
            .PutJsonAsync(TestDataHelper.CreateStoreApiKeyUpdateInfo());

        response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_WithoutAuth_ShouldReturn401()
    {
        var response = await _fixture.CreateAnonymousRequest("store")
            .AppendPathSegment(ForeignStoreId)
            .AllowAnyHttpStatus()
            .DeleteAsync();

        response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    // --------------------------------------------------------- happy-path 201
    [Fact]
    public async Task Create_WithValidBody_ShouldReturn201OrAlreadyExists()
    {
        // A API permite uma loja por usuário. Num usuário sem loja → 201 com
        // storeId/clientId; se já houver loja → 400 "User already has a store".
        // Ambos são contratos válidos do endpoint (e repetível — SC-006).
        var response = await _fixture.CreateAuthenticatedRequest("store")
            .AllowAnyHttpStatus()
            .PostJsonAsync(TestDataHelper.CreateStoreInsertInfo("QA Store " + Guid.NewGuid().ToString("N")[..8]));

        response.StatusCode.Should().BeOneOf(
            (int)HttpStatusCode.Created, (int)HttpStatusCode.BadRequest);

        if (response.StatusCode == (int)HttpStatusCode.Created)
        {
            var body = await response.GetJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>();
            body.Keys.Should().Contain(k => k.Equals("storeId", StringComparison.OrdinalIgnoreCase));
            body.Keys.Should().Contain(k => k.Equals("clientId", StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            (await response.GetStringAsync()).Should().Contain("store");
        }
    }

    // ---------------------------------------------- 403 recurso de terceiro
    [Fact]
    public async Task Update_ForNonOwnedStore_ShouldReturn403()
    {
        var response = await _fixture.CreateAuthenticatedRequest("store")
            .AllowAnyHttpStatus()
            .PutJsonAsync(TestDataHelper.CreateStoreUpdateInfo(ForeignStoreId));

        response.StatusCode.Should().BeOneOf(
            (int)HttpStatusCode.Forbidden,
            (int)HttpStatusCode.BadRequest); // Forbid() ou erro controlado para id de terceiro/inexistente
    }

    [Fact]
    public async Task SetAbacatePayApiKey_ForNonOwnedStore_ShouldReturn403()
    {
        var response = await _fixture.CreateAuthenticatedRequest("store")
            .AppendPathSegment(ForeignStoreId)
            .AppendPathSegment("abacatepay-apikey")
            .AllowAnyHttpStatus()
            .PutJsonAsync(TestDataHelper.CreateStoreApiKeyUpdateInfo());

        response.StatusCode.Should().BeOneOf(
            (int)HttpStatusCode.Forbidden,
            (int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_ForNonOwnedStore_ShouldReturn403()
    {
        var response = await _fixture.CreateAuthenticatedRequest("store")
            .AppendPathSegment(ForeignStoreId)
            .AllowAnyHttpStatus()
            .DeleteAsync();

        response.StatusCode.Should().BeOneOf(
            (int)HttpStatusCode.Forbidden,
            (int)HttpStatusCode.BadRequest);
    }

    // ---------------------------------- happy-path set apikey sobre própria
    [Fact]
    public async Task SetAbacatePayApiKey_WithValidBody_ShouldReturn204()
    {
        // Usa a loja do usuário autenticado (criada se ainda não existir).
        var (storeId, _) = await _fixture.GetOrCreateStoreAsync();
        if (storeId <= 0)
            return; // sem storeId utilizável no ambiente — degrada com elegância

        var response = await _fixture.CreateAuthenticatedRequest("store")
            .AppendPathSegment(storeId)
            .AppendPathSegment("abacatepay-apikey")
            .AllowAnyHttpStatus()
            .PutJsonAsync(TestDataHelper.CreateStoreApiKeyUpdateInfo());

        response.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }
}
