using System.Threading.Tasks;

namespace ProxyPay.Tests.Integration;

// =============================================================================
// Feature: 001-multi-tenant-fortuno — User Story 1 (Isolamento total de dados)
// Tasks:   T012 (listagem + cross-tenant GET por ID)
//          T013 (colisão de IDs entre tenants)
//
// Estas são as cenas de aceite descritas em spec.md US1. Elas estão como
// [Fact(Skip=...)] porque o projeto ProxyPay.Tests ainda não tem a
// infraestrutura de integração necessária:
//   - WebApplicationFactory<Startup> (Microsoft.AspNetCore.Mvc.Testing)
//   - Fixtures de dois bancos reais (proxypay_emagine_test, proxypay_fortuno_test)
//   - Seeds determinísticos (stores/invoices por tenant)
//   - Helper de autenticação que emite Basic token com o JwtSecret do tenant
//
// Próximo passo para habilitá-los:
//   1. Adicionar pacote `Microsoft.AspNetCore.Mvc.Testing` ao csproj
//   2. Criar IntegrationTestFixture (IClassFixture) com bootstrap dos dois bancos
//      + aplicação das migrations via MigrationRunner (ver T035).
//   3. Remover os Skip= dos [Fact] abaixo conforme cada cenário for suportado.
// =============================================================================
public class CrossTenantIsolationTests
{
    private const string PendingInfraReason =
        "Pendente infraestrutura de integração (WebApplicationFactory + 2 bancos reais). " +
        "Ver cabeçalho deste arquivo e task T012 em specs/001-multi-tenant-fortuno/tasks.md.";

    [Fact(Skip = PendingInfraReason)]
    public async Task ListStores_InFortuno_DoesNotIncludeEmagineStores()
    {
        // Arrange: semear store "EmagineStore" em emagine e store "FortunoStore" em fortuno.
        // Act:     GET /store autenticado em fortuno (X-Tenant-Id: fortuno + Basic token do fortuno).
        // Assert:  resposta contém apenas FortunoStore; EmagineStore não aparece.
        await Task.CompletedTask;
    }

    [Fact(Skip = PendingInfraReason)]
    public async Task GetStoreById_InFortuno_ForEmagineOwnedId_Returns404()
    {
        // Arrange: semear store com ID=42 em emagine; NÃO criar nenhum store em fortuno com ID=42.
        // Act:     GET /store/42 autenticado em fortuno.
        // Assert:  resposta 404 com body idêntico ao de qualquer ID inexistente;
        //          nenhum campo da store de emagine vaza no body.
        await Task.CompletedTask;
    }

    [Fact(Skip = PendingInfraReason)]
    public async Task UpdateStore_InFortuno_ForEmagineOwnedId_DoesNotMutateEmagine()
    {
        // Arrange: store ID=42 em emagine, conteúdo conhecido "OriginalName".
        // Act:     PUT /store/42 autenticado em fortuno com body { name: "Hacked" }.
        // Assert:  respostalança 404; store ID=42 em emagine ainda tem name="OriginalName"
        //          (confirmar lendo emagine direto no banco de teste).
        await Task.CompletedTask;
    }

    [Fact(Skip = PendingInfraReason)]
    public async Task CrossTenantInvoiceIdCollision_ResolvesToCorrectTenant()
    {
        // Arrange: inserir invoice com ID=100 em emagine (amount=1000) e
        //          inserir invoice com ID=100 em fortuno (amount=2000).
        //          Nota: IDs independentes por banco; colisão é possível e esperada.
        // Act:     GET /invoice/100 autenticado em fortuno.
        // Assert:  retorno contém amount=2000 (invoice de fortuno), nunca o de emagine.
        await Task.CompletedTask;
    }
}
