using System;
using Ganesha.Infra.Interfaces;
using Ganesha.Infra.Context;
using Ganesha.Domain.Core;
using Ganesha.Domain.Interfaces;

namespace Ganesha.Infra
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly GaneshaContext _ccsContext;
        private readonly ILogCore _log;

        public UnitOfWork(ILogCore log, GaneshaContext ccsContext)
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
