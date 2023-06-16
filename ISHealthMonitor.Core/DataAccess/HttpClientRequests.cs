using ISHealthMonitor.Core.Implementations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ISHealthMonitor.Core.Common;
using ISHealthMonitor.Core.Contracts;
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.Core;

namespace ISHealthMonitor.Core.DataAccess
{
    public class HttpClientRequests : IDisposable
    {
        private readonly ILogger<Rest> _logger;
        private string _baseURL;
        private string _token;
        private string _mediaType;// = "application/json";
        private HttpContentTypes _contentType;
        private readonly IConfiguration _config;
        public HttpClientRequests(IConfiguration config, string mediaType, HttpContentTypes contentType, ILogger<Rest> logger)
        {
            _baseURL = config["ConfluenceCloudApp:Endpoint"];
            _mediaType = mediaType;
            _contentType = contentType;
            _logger = logger;
            _config = config;
        }

        #region GET Request
        public async Task<T> GetHTTPAsync<T>(string url)
        {
            var strResponse = await GetHTTPAsync(url);

            return JsonConvert.DeserializeObject<T>(strResponse);
        }
        public async Task<string> GetHTTPAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_baseURL);
                byte[] cred = UTF8Encoding.UTF8.GetBytes(_config["ConfluenceCloudApp:ServiceAccount"] + ":" + _config["ConfluenceCloudApp:ServiceAccountToken"]);

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var responseMessage = await httpClient.GetAsync(_baseURL + url);

                var contents = responseMessage.Content.ReadAsStringAsync();
                return contents.Result;
            }
        }
        #endregion

        #region POST Request
        public async Task<string> PostAsync(string url, object input)
        {
            return await PostAsync(url, input.ToString(), HttpContentTypes.String);
        }
        public async Task<string> PostByteArrayAsync(string url, object input)
        {
            return await PostAsync(url, input.ToString(), HttpContentTypes.ByteArray);
        }
        public async Task<string> PostAsync(string url, string input, HttpContentTypes contentType)
        {
            var content = GetContent(input);
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_baseURL);
                byte[] cred = UTF8Encoding.UTF8.GetBytes(_config["ConfluenceCloudApp:ServiceAccount"] + ":" + _config["ConfluenceCloudApp:ServiceAccountToken"]);

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.PostAsync(url, content);

                var contents = response.Content.ReadAsStringAsync();
                return contents.Result;

            }
        }

        #endregion

        #region PUT Request
        public async Task<string> PutAsync(string url, object input)
        {
            return await PutAsync(url, input.ToString(), HttpContentTypes.String);
        }
        public async Task<string> PutByteArrayAsync(string url, object input)
        {
            return await PutAsync(url, input.ToString(), HttpContentTypes.ByteArray);
        }
        public async Task<string> PutAsync(string url, object input, HttpContentTypes contentType)
        {
            var content = GetContent(input);

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_baseURL);
                byte[] cred = UTF8Encoding.UTF8.GetBytes(_config["ConfluenceCloudApp:ServiceAccount"] + ":" + _config["ConfluenceCloudApp:ServiceAccountToken"]);

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.PutAsync(_baseURL + url, content);

                var contents = response.Content.ReadAsStringAsync();
                return contents.Result;

            }
        }

        #endregion

        private HttpContent GetContent(object input)
        {
            switch (_contentType)
            {
                case HttpContentTypes.String:
                    var inputJson = input.ToString();
                    return new StringContent(inputJson, Encoding.UTF8, _mediaType);
                case HttpContentTypes.Image:
                    var myByteArray = ConvertURItoByte(input.ToString());
                    MemoryStream stream = new MemoryStream(myByteArray);
                    return new StreamContent(stream);
                case HttpContentTypes.ByteArray:
                    var content = new ByteArrayContent(ConvertURItoByte(input.ToString()));
                    content.Headers.Add("Content-Type", MediaTypes.JPeg);
                    return content;
                default:
                    var inputJsonDefault = input.ToString();
                    return new StringContent(inputJsonDefault, Encoding.UTF8, _mediaType);
            }
        }
        private byte[] ConvertURItoByte(string imgUrl)
        {
            using (var wc = new WebClient())
            {
                _logger.LogInformation($"ConvertURItoByte RAW: imgUrl: {imgUrl}");
                var img = HttpUtility.UrlDecode(imgUrl);
                _logger.LogInformation($"ConvertURItoByte UrlDecode: imgUrl: {img}");
                _logger.LogInformation($"ConvertURItoByte REPLACE: imgUrl REPLACE: {img.Replace("\\", "/")}");
                return wc.DownloadData(img.Replace("\\", "/"));

            }
        }
        private static string ConvertToJsonString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(obj);
        }

        public void Dispose()
        {

        }
    }
}
