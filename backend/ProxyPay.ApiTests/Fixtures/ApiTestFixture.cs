using System.Text.Json;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using ProxyPay.ApiTests.Helpers;

namespace ProxyPay.ApiTests.Fixtures;

/// <summary>
/// Fixture compartilhada por toda a sessão de testes (uma instância via
/// ICollectionFixture). Autentica UMA vez no NAuth e reutiliza o token em
/// todas as requisições autenticadas.
///
/// Esquema de auth: NAuth Basic Authentication — header
/// "Authorization: Basic {token}" — combinado com o header multi-tenant
/// "X-Tenant-Id". Ver research.md R1/R2.
/// </summary>
public class ApiTestFixture : IAsyncLifetime
{
    public string BaseUrl { get; private set; } = string.Empty;
    public string AuthToken { get; private set; } = string.Empty;
    public string Tenant { get; private set; } = string.Empty;

    private IConfiguration _configuration = null!;
    private string _deviceFingerprint = "proxypay-apitests";
    private string _userAgent = "ProxyPay.ApiTests/1.0";
    private string _abacatePayApiKey = string.Empty;

    public async Task InitializeAsync()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        BaseUrl = RequireConfig("ApiBaseUrl");
        Tenant = RequireConfig("Auth:Tenant");
        var authBaseUrl = RequireConfig("Auth:BaseUrl");
        var email = RequireConfig("Auth:Email");
        var password = RequireConfig("Auth:Password");
        var loginEndpoint = _configuration["Auth:LoginEndpoint"] ?? "/user/loginWithEmail";
        _deviceFingerprint = _configuration["Auth:DeviceFingerprint"] ?? "proxypay-apitests";
        _userAgent = _configuration["Auth:UserAgent"] ?? "ProxyPay.ApiTests/1.0";
        _abacatePayApiKey = _configuration["Store:AbacatePayApiKey"] ?? string.Empty;

        try
        {
            var json = await new Url(authBaseUrl)
                .AppendPathSegment(loginEndpoint)
                .WithHeader("X-Tenant-Id", Tenant)
                .WithHeader("X-Device-Fingerprint", _deviceFingerprint)
                .WithHeader("User-Agent", _userAgent)
                .PostJsonAsync(new { email, password })
                .ReceiveString();

            AuthToken = ExtractToken(json);

            if (string.IsNullOrWhiteSpace(AuthToken))
                throw new Exception(
                    $"Login em {authBaseUrl}{loginEndpoint} (tenant '{Tenant}') não retornou token. " +
                    $"Resposta: {Truncate(json, 200)}");
        }
        catch (FlurlHttpException ex)
        {
            var responseBody = await SafeReadBody(ex);
            throw new Exception(
                $"Falha ao autenticar para os testes de API. Status: {ex.StatusCode}. " +
                $"POST {authBaseUrl}{loginEndpoint} | X-Tenant-Id: '{Tenant}' | " +
                $"X-Device-Fingerprint: '{_deviceFingerprint}'. " +
                $"Resposta do NAuth: {Truncate(responseBody, 500)}. " +
                $"Confirme se a NAuth API está em execução em {authBaseUrl}, se o endpoint " +
                $"'{loginEndpoint}', o tenant e as credenciais estão corretos.", ex);
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Requisição com auth NAuth Basic + tenant + device fingerprint + user agent.</summary>
    public IFlurlRequest CreateAuthenticatedRequest(string path) =>
        new Url(BaseUrl)
            .AppendPathSegment(path)
            .WithHeader("Authorization", $"Basic {AuthToken}")
            .WithHeader("X-Tenant-Id", Tenant)
            .WithHeader("X-Device-Fingerprint", _deviceFingerprint)
            .WithHeader("User-Agent", _userAgent);

    /// <summary>Requisição sem auth, mantendo o tenant + device fingerprint + user agent.</summary>
    public IFlurlRequest CreateAnonymousRequest(string path) =>
        new Url(BaseUrl)
            .AppendPathSegment(path)
            .WithHeader("X-Tenant-Id", Tenant)
            .WithHeader("X-Device-Fingerprint", _deviceFingerprint)
            .WithHeader("User-Agent", _userAgent);

    private (long StoreId, string ClientId)? _cachedStore;

    /// <summary>
    /// Garante uma loja para o usuário autenticado e devolve (StoreId, ClientId).
    /// A API permite **uma loja por usuário**, então: tenta criar; se já existir
    /// ("User already has a store"), busca a loja existente via GraphQL `myStore`.
    /// Se houver AbacatePay API key configurada, define-a na loja para habilitar
    /// os fluxos de pagamento no sandbox. O resultado é cacheado por sessão.
    /// </summary>
    public async Task<(long StoreId, string ClientId)> GetOrCreateStoreAsync()
    {
        if (_cachedStore is not null)
            return _cachedStore.Value;

        long storeId;
        string clientId;

        var createResponse = await CreateAuthenticatedRequest("store")
            .AllowAnyHttpStatus()
            .PostJsonAsync(TestDataHelper.CreateStoreInsertInfo(
                "QA Store " + Guid.NewGuid().ToString("N").Substring(0, 8)));

        if (createResponse.StatusCode == (int)System.Net.HttpStatusCode.Created)
        {
            var json = await createResponse.GetStringAsync();
            storeId = ExtractLong(json, "storeId", "StoreId") ?? 0;
            clientId = ExtractString(json, "clientId", "ClientId") ?? string.Empty;
        }
        else
        {
            // Usuário já possui loja (ou outro motivo) — busca a loja existente.
            (storeId, clientId) = await FetchMyStoreAsync();
            if (string.IsNullOrWhiteSpace(clientId))
            {
                var body = await createResponse.GetStringAsync();
                throw new Exception(
                    $"Não foi possível criar nem localizar a loja do usuário. " +
                    $"POST /store → {createResponse.StatusCode}: {Truncate(body, 200)}");
            }
        }

        // Habilita os fluxos de pagamento configurando a API key da AbacatePay (sandbox).
        if (storeId > 0 && !string.IsNullOrWhiteSpace(_abacatePayApiKey))
        {
            await CreateAuthenticatedRequest("store")
                .AppendPathSegment(storeId)
                .AppendPathSegment("abacatepay-apikey")
                .AllowAnyHttpStatus()
                .PutJsonAsync(new { apiKey = _abacatePayApiKey });
        }

        _cachedStore = (storeId, clientId);
        return _cachedStore.Value;
    }

    /// <summary>
    /// Busca a loja do usuário autenticado via GraphQL `myStore` e retorna
    /// (StoreId, ClientId). Retorna (0, "") se o usuário não tiver loja.
    /// </summary>
    public async Task<(long StoreId, string ClientId)> FetchMyStoreAsync()
    {
        var response = await CreateAuthenticatedRequest("graphql")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new { query = "{ myStore { storeId clientId } }" });

        var json = await response.GetStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var data)
                && data.TryGetProperty("myStore", out var myStore)
                && myStore.ValueKind == JsonValueKind.Array
                && myStore.GetArrayLength() > 0)
            {
                var first = myStore[0];
                var storeId = first.TryGetProperty("storeId", out var sid) && sid.TryGetInt64(out var n) ? n : 0;
                var clientId = first.TryGetProperty("clientId", out var cid) && cid.ValueKind == JsonValueKind.String
                    ? cid.GetString() ?? string.Empty
                    : string.Empty;
                return (storeId, clientId);
            }
        }
        catch (JsonException)
        {
        }

        return (0, string.Empty);
    }

    private string RequireConfig(string key)
    {
        var value = _configuration[key]
            ?? throw new Exception($"Chave de configuração obrigatória ausente: '{key}'.");

        if (value.StartsWith("REPLACE_VIA_ENV_"))
        {
            var envVar = value.Substring("REPLACE_VIA_ENV_".Length);
            throw new Exception(
                $"A chave de configuração '{key}' ainda contém o placeholder. " +
                $"Exporte a variável de ambiente '{envVar}' antes de rodar os testes.");
        }

        return value;
    }

    /// <summary>
    /// Extrai o token da resposta de login de forma tolerante a variações de
    /// nome de campo (token / accessToken / access_token), em qualquer nível.
    /// </summary>
    private static string ExtractToken(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return FindToken(doc.RootElement) ?? string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static string? FindToken(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                var name = prop.Name.ToLowerInvariant();
                if ((name == "token" || name == "accesstoken" || name == "access_token")
                    && prop.Value.ValueKind == JsonValueKind.String)
                {
                    var value = prop.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }

                var nested = FindToken(prop.Value);
                if (!string.IsNullOrWhiteSpace(nested))
                    return nested;
            }
        }

        return null;
    }

    private static string? ExtractString(string json, params string[] keys)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;
            foreach (var key in keys)
                if (doc.RootElement.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.String)
                    return el.GetString();
        }
        catch (JsonException)
        {
        }
        return null;
    }

    private static long? ExtractLong(string json, params string[] keys)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;
            foreach (var key in keys)
            {
                if (!doc.RootElement.TryGetProperty(key, out var el))
                    continue;
                if (el.ValueKind == JsonValueKind.Number && el.TryGetInt64(out var n))
                    return n;
                if (el.ValueKind == JsonValueKind.String && long.TryParse(el.GetString(), out var s))
                    return s;
            }
        }
        catch (JsonException)
        {
        }
        return null;
    }

    private static async Task<string> SafeReadBody(FlurlHttpException ex)
    {
        try
        {
            return await ex.GetResponseStringAsync() ?? string.Empty;
        }
        catch
        {
            return "(sem corpo legível)";
        }
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value.Substring(0, max) + "...";
}
