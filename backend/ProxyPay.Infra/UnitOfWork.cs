using System;
using ProxyPay.Infra.Interfaces;
using ProxyPay.Infra.Context;
using ProxyPay.Domain.Core;
using ProxyPay.Domain.Interfaces;

namespace ProxyPay.Infra
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly ProxyPayContext _ccsContext;
        private readonly ILogCore _log;

        public UnitOfWork(ILogCore log, ProxyPayContext ccsContext)
        {
            this._ccsContext = ccsContext;
            _log = log;
        }

        public ITransaction BeginTransaction()
        {
            try
            {
                _log.Log("Iniciando bloco de transação.", Levels.Trace);
                return new TransactionDisposable(_log, _ccsContext.Database.BeginTransaction());
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
