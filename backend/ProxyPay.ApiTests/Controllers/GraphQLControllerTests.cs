using System.Net;
using System.Text.Json;
using FluentAssertions;
using Flurl.Http;
using ProxyPay.ApiTests.Fixtures;
using ProxyPay.ApiTests.Helpers;

namespace ProxyPay.ApiTests.Controllers;

/// <summary>
/// Testes do endpoint GraphQL (POST /graphql) e do stub de docs
/// (POST /api/graphql-docs, [Authorize]). US4 (P3).
/// Valida o CONTRATO das consultas autenticadas: autenticada → 200 com "data"
/// e sem "errors"; anônima → rejeitada (sem dados / erro de autorização).
/// </summary>
[Collection("ApiTests")]
public class GraphQLControllerTests
{
    private readonly ApiTestFixture _fixture;

    public GraphQLControllerTests(ApiTestFixture fixture) => _fixture = fixture;

    // -------------------------------------------------- consultas autenticadas
    [Fact]
    public Task Graphql_MyStore_WithAuth_ShouldReturnData() =>
        AssertAuthenticatedQuerySucceeds(GraphQLQueries.MyStore);

    [Fact]
    public Task Graphql_MyInvoices_WithAuth_ShouldReturnData() =>
        AssertAuthenticatedQuerySucceeds(GraphQLQueries.MyInvoices);

    [Fact]
    public Task Graphql_MyInvoiceByNumber_WithAuth_ShouldReturnData() =>
        AssertAuthenticatedQuerySucceeds(GraphQLQueries.MyInvoiceByNumber);

    [Fact]
    public Task Graphql_MyTransactions_WithAuth_ShouldReturnData() =>
        AssertAuthenticatedQuerySucceeds(GraphQLQueries.MyTransactions);

    [Fact]
    public Task Graphql_MyBalance_WithAuth_ShouldReturnData() =>
        AssertAuthenticatedQuerySucceeds(GraphQLQueries.MyBalance);

    [Fact]
    public Task Graphql_MyCustomers_WithAuth_ShouldReturnData() =>
        AssertAuthenticatedQuerySucceeds(GraphQLQueries.MyCustomers);

    // ---------------------------------------------------------- anônimo negado
    [Fact]
    public async Task Graphql_MyStore_WithoutAuth_ShouldBeUnauthorized()
    {
        var response = await _fixture.CreateAnonymousRequest("graphql")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new { query = GraphQLQueries.MyStore });

        if (response.StatusCode != (int)HttpStatusCode.OK)
        {
            response.StatusCode.Should().BeOneOf(
                (int)HttpStatusCode.Unauthorized, (int)HttpStatusCode.Forbidden);
            return;
        }

        // HotChocolate pode responder 200 com erro de autorização no corpo.
        using var doc = JsonDocument.Parse(await response.GetStringAsync());
        var hasData = doc.RootElement.TryGetProperty("data", out var data)
                      && data.ValueKind != JsonValueKind.Null;
        var hasErrors = doc.RootElement.TryGetProperty("errors", out var errors)
                        && errors.ValueKind == JsonValueKind.Array
                        && errors.GetArrayLength() > 0;

        (hasErrors || !hasData).Should().BeTrue(
            "uma consulta anônima a um campo [Authorize] não deve retornar dados");
    }

    // --------------------------------------------------- stub /api/graphql-docs
    [Fact]
    public async Task GraphqlDocs_WithoutAuth_ShouldReturn401()
    {
        var response = await _fixture.CreateAnonymousRequest("api/graphql-docs")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new { query = GraphQLQueries.MyStore });

        response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GraphqlDocs_WithAuth_ShouldReturn200()
    {
        // O DTO GraphQLRequest da API (nullable enable) exige os 3 campos:
        // query, operationName e variables. O endpoint é apenas um stub de
        // documentação Swagger que retorna 200 sem executar a query.
        var response = await _fixture.CreateAuthenticatedRequest("api/graphql-docs")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                query = GraphQLQueries.MyStore,
                operationName = "ApiTestDocs",
                variables = new { }
            });

        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    // ------------------------------------------------------------------ helper
    private async Task AssertAuthenticatedQuerySucceeds(string query)
    {
        var response = await _fixture.CreateAuthenticatedRequest("graphql")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new { query });

        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.GetStringAsync());

        var hasErrors = doc.RootElement.TryGetProperty("errors", out var errors)
                        && errors.ValueKind == JsonValueKind.Array
                        && errors.GetArrayLength() > 0;
        hasErrors.Should().BeFalse(
            "a consulta autenticada não deve produzir erros de execução GraphQL");

        doc.RootElement.TryGetProperty("data", out var data).Should().BeTrue();
        data.ValueKind.Should().NotBe(JsonValueKind.Null);
    }
}
