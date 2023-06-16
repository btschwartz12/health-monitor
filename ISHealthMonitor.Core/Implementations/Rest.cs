using Azure.Core;
using ISHealthMonitor.Core.Common;
using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Implementations
{
    public class Rest : IRest
    {
        private readonly ILogger<Rest> _logger;
        private readonly IConfiguration _config;
        public Rest(ILogger<Rest> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

        }

        public Task<string> GetHttpContent(string url)
        {
            using (var client = new HttpClientRequests(_config, MediaTypes.Json, HttpContentTypes.String, _logger))
            {
                return client.GetHTTPAsync(url);
            }
        }

        public Task<string> PutHttpContent(string url, object bodycontent, HttpContentTypes types)
        {


            using (var client = new HttpClientRequests(_config, MediaTypes.Json, types, _logger))
            {
                switch (types)
                {
                    case HttpContentTypes.String:
                        return client.PutAsync(url, bodycontent);
                    case HttpContentTypes.ByteArray:
                        return client.PutByteArrayAsync(url, bodycontent);
                    default:
                        return client.PutAsync(url, bodycontent);
                }
            }
        }

        public Task<string> PostHttpContent(string url, object bodycontent, HttpContentTypes types)
        {

            using (var client = new HttpClientRequests(_config, MediaTypes.Json, types, _logger))
            {
                switch (types)
                {
                    case HttpContentTypes.String:
                        return client.PostAsync(url, bodycontent);
                    case HttpContentTypes.ByteArray:
                        return client.PostByteArrayAsync(url, bodycontent);
                    default:
                        return client.PostAsync(url, bodycontent);
                }
            }
        }


    }
}
