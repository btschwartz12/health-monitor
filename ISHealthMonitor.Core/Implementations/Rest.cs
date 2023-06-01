using ISHealthMonitor.Core.Common;
using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Implementations
{
    public class Rest : IRest
    {
        private readonly ILogger<Rest> _logger;
        private readonly IConfiguration _config;
        private string? _clientId;
        private string? _clientSecret;
        private string? _tenantId;
        private string _accessToken;
        private WebServiceSettings _settings;
        public Rest(ILogger<Rest> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _clientId = _config["GraphAPICreds:ClientID"];
            _clientSecret = _config["GraphAPICreds:Secret"];
            _tenantId = _config["GraphAPICreds:Tenant"];

            _settings = new WebServiceSettings(_clientId, _clientSecret, _tenantId);
        }
        public async Task<string> GetHttpContentWithToken(string url)
        {
            var gs = new AzureADService(_settings, _logger);
            _accessToken = await gs.GetAccessToken();

            using (var client = new HttpClientRequests(GraphEndpoints.BaseURL, _accessToken, MediaTypes.Json, HttpContentTypes.String, _logger))
            {
                return client.GetAsync(url).Result;
            }
        }
        public async Task<string> PutHttpContentWithToken(string url, string bodycontent, HttpContentTypes types)
        {
            var gs = new AzureADService(_settings, _logger);
            _accessToken = await gs.GetAccessToken();

            using (var client = new HttpClientRequests(GraphEndpoints.BaseURL, _accessToken, MediaTypes.Json, types, _logger))
            {
                switch (types)
                {
                    case HttpContentTypes.String:
                        return client.PutAsync(url, bodycontent).Result;
                    case HttpContentTypes.ByteArray:
                        return client.PutByteArrayAsync(url, bodycontent).Result;
                    default:
                        return client.PutAsync(url, bodycontent).Result;
                }
            }
        }

        public async Task<string> PostHttpContentWithToken(string url, string bodycontent, HttpContentTypes types)
        {
            var gs = new AzureADService(_settings, _logger);
            _accessToken = await gs.GetAccessToken();

            using (var client = new HttpClientRequests(GraphEndpoints.BaseURL, _accessToken, MediaTypes.Json, types, _logger))
            {
                switch (types)
                {
                    case HttpContentTypes.String:
                        return client.PostAsync(url, bodycontent).Result;
                    case HttpContentTypes.ByteArray:
                        return client.PostByteArrayAsync(url, bodycontent).Result;
                    default:
                        return client.PostAsync(url, bodycontent).Result;
                }
            }
        }

        public Task<string> GetHttpContent(string url)
        {
            throw new NotImplementedException();
        }

        public Task<string> PutHttpContent(string url, string bodycontent, HttpContentTypes types)
        {
            throw new NotImplementedException();
        }

        public Task<string> PostHttpContent(string url, string bodycontent, HttpContentTypes types)
        {
            throw new NotImplementedException();
        }
    }
}
