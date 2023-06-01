using ISHealthMonitor.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.DataAccess
{
    public class AzureADService
    {
        private string _clientId;

        private string _secret;

        private string _tenantId;

        private readonly ILogger<object> _logger;

        private string[] _scopes = new string[] { "https://graph.microsoft.com/.default" };
        public AzureADService(WebServiceSettings settings, ILogger<object> logger)
        {
            _logger = logger;

            if (settings != null)
            {
                _clientId = settings.ClientID;
                _secret = settings.ClientSecret;
                _tenantId = settings.TenantID;
            }
        }
        public async Task<string> GetAccessToken()
        {
            var builder = ConfidentialClientApplicationBuilder
                .Create(_clientId)
                .WithClientSecret(_secret)
                .WithTenantId(_tenantId)
                .Build();

            var acquiredTokenResult = builder.AcquireTokenForClient(_scopes);
            var tokenResult = await acquiredTokenResult.ExecuteAsync();
            return tokenResult.AccessToken;
        }

    }
}
