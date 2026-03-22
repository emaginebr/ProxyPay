using System;
using Ganesha.Domain.Core;
using Microsoft.Extensions.Logging;

namespace Ganesha.Domain.Interfaces
{
    public interface ILogCore
    {
        void Log(string message, Levels level);
    }
}
