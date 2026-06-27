namespace ProxyPay.ApiTests.Helpers;

/// <summary>
/// Consultas GraphQL autenticadas exercitadas pelos testes
/// (ver ProxyPay.GraphQL/Admin/AdminQuery.cs).
///
/// Notas de schema (HotChocolate, convenção default):
/// - O resolver GetMyStore expõe o campo singular "myStore" (lista de Store).
/// - Campos com [UseOffsetPaging] (myInvoices/myTransactions/myCustomers)
///   expõem "items".
/// - myInvoiceByNumber exige o argumento "invoiceNumber".
/// - myBalance retorna o objeto BalanceSummary.
///
/// Selecionamos "__typename" (sempre válido) nas listas/objetos de entidade
/// para validar o CONTRATO da consulta sem depender de nomes de campo das
/// entidades — exceto myBalance, cujos campos são conhecidos (BalanceSummary).
/// </summary>
public static class GraphQLQueries
{
    public const string MyStore = "{ myStore { __typename } }";

    public const string MyInvoices = "{ myInvoices { items { __typename } } }";

    public const string MyInvoiceByNumber =
        "{ myInvoiceByNumber(invoiceNumber: \"__qa_nonexistent__\") { __typename } }";

    public const string MyTransactions = "{ myTransactions { items { __typename } } }";

    public const string MyBalance =
        "{ myBalance { balance totalCredits totalDebits transactionCount } }";

    public const string MyCustomers = "{ myCustomers { items { __typename } } }";
}
