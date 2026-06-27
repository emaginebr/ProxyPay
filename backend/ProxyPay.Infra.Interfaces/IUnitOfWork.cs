namespace ProxyPay.Infra.Interfaces
{
    public interface IUnitOfWork
    {
        ITransaction BeginTransaction();
    }
}
