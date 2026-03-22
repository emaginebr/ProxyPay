using Microsoft.Extensions.Options;
using Ganesha.DTO.Settings;
using System;

namespace Ganesha.ACL.Core
{
    public abstract class BaseClient
    {
        protected readonly HttpClient _httpClient;
        protected readonly IOptions<GaneshaSetting> _nsalesSetting;

        public BaseClient(IOptions<GaneshaSetting> nsalesSetting)
        {
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
            _nsalesSetting = nsalesSetting;
        }
    }
}
