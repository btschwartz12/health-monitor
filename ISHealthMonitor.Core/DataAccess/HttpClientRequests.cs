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

namespace ISHealthMonitor.Core.DataAccess
{
    public class HttpClientRequests : IDisposable
    {
        private readonly ILogger<Rest> _logger;
        private HttpClient _httpClient;
        private string _baseURL;
        private string _token;
        private string _mediaType;// = "application/json";
        private HttpContentTypes _contentType;

        public HttpClientRequests(string baseUrl, string token, string mediaType, HttpContentTypes contentType, ILogger<Rest> logger)
        {
            _baseURL = baseUrl;
            _token = token;
            _mediaType = mediaType;
            _contentType = contentType;
            _logger = logger;
        }
        #region GET Request
        public async Task<T> GetAsync<T>(string url)
        {
            var strResponse = await GetAsync(url);

            return JsonConvert.DeserializeObject<T>(strResponse);
        }

        public async Task<string> GetAsync(string url)
        {
            _httpClient = GetMyHttpClient();

            using (var response = await _httpClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var contents = response.Content.ReadAsStringAsync();
                    //log error
                    return contents.Result;
                }
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
            _httpClient = GetMyHttpClient();
            var content = GetContent(input);
            //# true if groupMembershipClaims is "SecurityGroup", false if it's "All"

            using (var response = await _httpClient.PostAsync(url, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var contents = response.Content.ReadAsStringAsync();
                    //log error
                    return contents.Result;
                }
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
            //var content = new ByteArrayContent(ConvertURItoByte(input.ToString()));
            //content.Headers.Add("Content-Type", "image/jpeg");

            return await PutAsync(url, input.ToString(), HttpContentTypes.ByteArray);
        }
        public async Task<string> PutAsync(string url, string input, HttpContentTypes contentType)
        {
            _httpClient = GetMyHttpClient();
            var content = GetContent(input);

            using (var response = await _httpClient.PutAsync(url, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var contents = response.Content.ReadAsStringAsync();
                    //log error
                    return contents.Result;
                }
            }
        }

        #endregion

        private HttpClient GetMyHttpClient()
        {
            if (_httpClient == null)
            {
                CreateHttpClient();
            }
            return _httpClient;
        }

        private void CreateHttpClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseURL);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_mediaType));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        private HttpContent GetContent(object input)
        {
            switch (_contentType)
            {
                case HttpContentTypes.String:
                    return new StringContent(ConvertToJsonString(input), Encoding.UTF8, _mediaType);
                case HttpContentTypes.Image:
                    var myByteArray = ConvertURItoByte(input.ToString());
                    MemoryStream stream = new MemoryStream(myByteArray);
                    return new StreamContent(stream);
                case HttpContentTypes.ByteArray:
                    var content = new ByteArrayContent(ConvertURItoByte(input.ToString()));
                    content.Headers.Add("Content-Type", MediaTypes.JPeg);
                    return content;
                default:
                    return new StringContent(input.ToString(), Encoding.UTF8, _mediaType);
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
            _httpClient?.Dispose();
        }

    }
}
