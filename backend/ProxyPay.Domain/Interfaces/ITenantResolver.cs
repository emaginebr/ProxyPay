namespace ProxyPay.Domain.Interfaces
{
    public interface ITenantResolver
    {
        string TenantId { get; }
        string ConnectionString { get; }
        string JwtSecret { get; }
        string BucketName { get; }
    }
}
