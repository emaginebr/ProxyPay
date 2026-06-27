using System;
using ProxyPay.Domain.Core;
using Microsoft.Extensions.Logging;

namespace ProxyPay.Domain.Interfaces
{
    public interface ILogCore
    {
        void Log(string message, Levels level);
    }
}
