using System;
using System.Threading.Tasks;

namespace ProxyPay.Infra.Interfaces
{
    public interface ICryptoUtils
    {
        bool CheckPersonalSignature(string phrase, string signature, string userAddress);
        Task<string> TesteConnection(string contractAddress);
    }
}
