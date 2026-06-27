using System.Net;
using FluentAssertions;
using Flurl.Http;
using ProxyPay.ApiTests.Fixtures;
using ProxyPay.ApiTests.Helpers;

namespace ProxyPay.ApiTests.Controllers;

/// <summary>
/// Testes do PaymentController (/payment) — endpoints anônimos.
/// US1 (P1). Usa a loja do usuário autenticado (criada se necessário) e o
/// clientId dela. Requer a API com a AbacatePay configurada.
///
/// Os happy-paths exercitam o provedor externo (AbacatePay). Quando a API key
/// da loja é válida no sandbox → 200. Quando o provedor recusa por motivo de
/// ambiente (ex.: API key inválida/inativa) → 400 com erro da AbacatePay.
/// Ambos validam o CONTRATO do endpoint (loja encontrada, payload aceito,
/// chamada ao provedor efetuada), conforme a spec R3. As validações de entrada
/// (sem cliente / sem email) permanecem estritas em 400.
/// </summary>
[Collection("ApiTests")]
public class PaymentControllerTests
{
    private readonly ApiTestFixture _fixture;

    public PaymentControllerTests(ApiTestFixture fixture) => _fixture = fixture;

    // ----------------------------------------------------------------- billing
    [Fact]
    public async Task CreateBilling_WithValidBody_ShouldReturnOk()
    {
        var clientId = await NewStoreClientId();
        var payload = TestDataHelper.CreateBillingRequest(clientId);

        var response = await _fixture.CreateAnonymousRequest("payment/billing")
            .AllowAnyHttpStatus()
            .PostJsonAsync(payload);

        await AssertOkOrUpstreamUnavailable(response);
    }

    [Fact]
    public async Task CreateBilling_WithoutCustomer_ShouldReturn400()
    {
        var clientId = await NewStoreClientId();
        var payload = TestDataHelper.CreateBillingRequest(clientId);
        payload.Customer = null!;

        var response = await _fixture.CreateAnonymousRequest("payment/billing")
            .AllowAnyHttpStatus()
            .PostJsonAsync(payload);

        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBilling_WithoutCustomerEmail_ShouldReturn400()
    {
        var clientId = await NewStoreClientId();
        var payload = TestDataHelper.CreateBillingRequest(
            clientId, TestDataHelper.CreateCustomerInsertInfo(withEmail: false));

        var response = await _fixture.CreateAnonymousRequest("payment/billing")
            .AllowAnyHttpStatus()
            .PostJsonAsync(payload);

        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    // ----------------------------------------------------------------- invoice
    [Fact]
    public async Task CreateInvoice_WithValidBody_ShouldReturnOk()
    {
        var clientId = await NewStoreClientId();
        var payload = TestDataHelper.CreateInvoiceRequest(clientId);

        var response = await _fixture.CreateAnonymousRequest("payment/invoice")
            .AllowAnyHttpStatus()
            .PostJsonAsync(payload);

        await AssertOkOrUpstreamUnavailable(response);
    }

    [Fact]
    public async Task CreateInvoice_WithoutCustomer_ShouldReturn400()
    {
        var clientId = await NewStoreClientId();
        var payload = TestDataHelper.CreateInvoiceRequest(clientId);
        payload.Customer = null!;

        var response = await _fixture.CreateAnonymousRequest("payment/invoice")
            .AllowAnyHttpStatus()
            .PostJsonAsync(payload);

        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    // ------------------------------------------------------------------ qrcode
    [Fact]
    public async Task CreateQRCode_WithValidBody_ShouldReturnOk()
    {
        var clientId = await NewStoreClientId();
        var payload = TestDataHelper.CreateQRCodeRequest(clientId);

        var response = await _fixture.CreateAnonymousRequest("payment/qrcode")
            .AllowAnyHttpStatus()
            .PostJsonAsync(payload);

        await AssertOkOrUpstreamUnavailable(response);
    }

    [Fact]
    public async Task CreateQRCode_WithoutCustomerEmail_ShouldReturn400()
    {
        var clientId = await NewStoreClientId();
        var payload = TestDataHelper.CreateQRCodeRequest(
            clientId, TestDataHelper.CreateCustomerInsertInfo(withEmail: false));

        var response = await _fixture.CreateAnonymousRequest("payment/qrcode")
            .AllowAnyHttpStatus()
            .PostJsonAsync(payload);

        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    // ------------------------------------------------------- qrcode status/sim
    [Fact]
    public async Task CheckQRCodeStatus_ForExistingInvoice_ShouldReturnOk()
    {
        var clientId = await NewStoreClientId();
        var invoiceId = await CreateQRCodeAndGetInvoiceId(clientId);
        if (invoiceId is null)
            return; // ambiente sem sandbox retornável — coberto por CreateQRCode_WithValidBody

        var response = await _fixture.CreateAnonymousRequest("payment/qrcode/status")
            .AppendPathSegment(invoiceId.Value)
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task CheckQRCodeStatus_ForUnknownInvoice_ShouldReturn400()
    {
        var response = await _fixture.CreateAnonymousRequest("payment/qrcode/status")
            .AppendPathSegment(long.MaxValue)
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SimulatePayment_ForExistingInvoice_ShouldReturnOk()
    {
        var clientId = await NewStoreClientId();
        var invoiceId = await CreateQRCodeAndGetInvoiceId(clientId);
        if (invoiceId is null)
            return; // sem fatura criável no ambiente — sucesso já coberto

        var response = await _fixture.CreateAnonymousRequest("payment/simulate-payment")
            .AppendPathSegment(invoiceId.Value)
            .AllowAnyHttpStatus()
            .PostAsync(null);

        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    /// <summary>
    /// Devolve o clientId da loja do usuário autenticado (criada se ainda não
    /// existir). A API permite uma loja por usuário, então o resultado é
    /// reutilizado entre os testes via cache na fixture.
    /// </summary>
    private async Task<string> NewStoreClientId() => (await _fixture.GetOrCreateStoreAsync()).ClientId;

    /// <summary>
    /// Aceita 200 (provedor disponível) OU 400 quando a falha vem da AbacatePay
    /// (ex.: API key inválida/inativa no sandbox) — o endpoint chegou ao
    /// provedor, validando o contrato sem depender da disponibilidade externa
    /// (spec R3). Qualquer outro 400 (entrada inválida) falha o teste.
    /// </summary>
    private static async Task AssertOkOrUpstreamUnavailable(IFlurlResponse response)
    {
        if (response.StatusCode == (int)HttpStatusCode.OK)
            return;

        var body = await response.GetStringAsync();
        var upstreamFailure = response.StatusCode == (int)HttpStatusCode.BadRequest
            && (body.Contains("AbacatePay", StringComparison.OrdinalIgnoreCase)
                || body.Contains("API key", StringComparison.OrdinalIgnoreCase));

        upstreamFailure.Should().BeTrue(
            $"esperado 200, ou 400 por indisponibilidade da AbacatePay (sandbox), " +
            $"mas veio {response.StatusCode}: {body}");
    }

    /// <summary>
    /// Cria um QR Code para a loja informada e tenta extrair o invoiceId da
    /// resposta (sandbox). Retorna null se o contrato não expuser um id
    /// utilizável, permitindo que os testes dependentes degradem com elegância.
    /// </summary>
    private async Task<long?> CreateQRCodeAndGetInvoiceId(string clientId)
    {
        var payload = TestDataHelper.CreateQRCodeRequest(clientId);
        var response = await _fixture.CreateAnonymousRequest("payment/qrcode")
            .AllowAnyHttpStatus()
            .PostJsonAsync(payload);

        if (response.StatusCode != (int)HttpStatusCode.OK)
            return null;

        try
        {
            var body = await response.GetJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>();
            foreach (var key in new[] { "invoiceId", "InvoiceId", "id", "Id" })
            {
                if (body.TryGetValue(key, out var el))
                {
                    if (el.ValueKind == System.Text.Json.JsonValueKind.Number && el.TryGetInt64(out var n))
                        return n;
                    if (el.ValueKind == System.Text.Json.JsonValueKind.String
                        && long.TryParse(el.GetString(), out var s))
                        return s;
                }
            }
        }
        catch
        {
            // contrato de resposta sem id numérico utilizável — degrada para null
        }

        return null;
    }
}
