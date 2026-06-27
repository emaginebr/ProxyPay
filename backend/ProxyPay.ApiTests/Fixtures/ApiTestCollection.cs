namespace ProxyPay.ApiTests.Fixtures;

/// <summary>
/// Define a collection que compartilha uma única <see cref="ApiTestFixture"/>
/// entre todas as classes de teste (login NAuth único por sessão).
/// </summary>
[CollectionDefinition("ApiTests")]
public class ApiTestCollection : ICollectionFixture<ApiTestFixture>
{
}
