using ISHealthMonitor.Core.Helpers.WorkOrder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ISHealthMonitor.Core.DataAccess;
using System.Runtime;
using System.Text;
using System.Text.Json;

namespace ISHealthMonitor.Core.Model
{
    public class UnityRestAPIAccess
    {
        private readonly ILogger<object> _logger;
        private readonly IConfiguration _config;
        public UnityRestAPIAccess(ILogger<object> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
        public List<ExpandedWorkviewObject> GetWorkViewObject(string appName, string filter)
        {
            using (var client = new HttpClient())
            {
                var sfAccess = new UnityRestAPIToken(_logger, _config);

                if (sfAccess.AuthenticationToken.AccessToken == "Failed to Authenticate")
                {
                    throw new Exception("Could not authenticate with Salesforce.");
                }

                var baseApiUrl = _config.GetSection("UnityRestAPI")["unityRestApiURL"];
                var datasource = _config.GetSection("UnityRestAPI")["unityRestApiDatasource"];

                string url = baseApiUrl + datasource + "/";

                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", sfAccess.AuthenticationToken.AccessToken);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var location = appName + "/filter/" + filter;
                var response = client.GetAsync(location).Result;
                if (response.IsSuccessStatusCode)
                {
                    var contents = response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ExpandedWorkviewObject>>(contents.Result);
                }
                else
                {
                    var contents = response.Content.ReadAsStringAsync();
                    return null;
                }

            }
        }

        public async Task<string> CreateWorkViewObject(string appName, string className, string wvObject)
        {
            using (var client = new HttpClient())
            {
                var sfAccess = new UnityRestAPIToken(_logger, _config);

                if (sfAccess.AuthenticationToken.AccessToken == "Failed to Authenticate")
                {
                    throw new Exception("Could not authenticate with Salesforce.");
                }

                var baseApiUrl = _config.GetSection("UnityRestAPI")["unityRestApiURL"];
                var datasource = _config.GetSection("UnityRestAPI")["unityRestApiDatasource"];

                string url = baseApiUrl + datasource + "/create";

                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", sfAccess.AuthenticationToken.AccessToken);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(wvObject, Encoding.UTF8, "application/json");
                var response = client.PostAsync("?appName=" + appName + "&className=" + className, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    var contents = response.Content.ReadAsStringAsync();
                    return contents.Result;
                }
                else
                {
                    throw new Exception(response.ToString());
                }

            }
        }


        public async Task<int> GetRequestorId(string email)
        {
            var requestBody = new RequestBody
            {
                Constraints = new List<Constraint>
                {
                    new Constraint
                    {
                        DottedAddress = "email",
                        Operator = "equal",
                        Value = email,
                        WorkviewOperator = 0,
                    },
                    new Constraint
                    {
                        DottedAddress = "Active",
                        Operator = "equal",
                        Value = true,
                        WorkviewOperator = 0,
                    }
                }
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);


            using (var client = new HttpClient())
            {
                var sfAccess = new UnityRestAPIToken(_logger, _config);

                if (sfAccess.AuthenticationToken.AccessToken == "Failed to Authenticate")
                {
                    throw new Exception("Could not authenticate with Salesforce.");
                }

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", sfAccess.AuthenticationToken.AccessToken);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var baseApiUrl = _config.GetSection("UnityRestAPI")["unityRestApiRequestorURL"];
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var url = baseApiUrl + "results=1";

                var response = client.PostAsync(url, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    var contents = response.Content.ReadAsStringAsync().Result;
                    
                    JsonDocument doc = JsonDocument.Parse(contents);

                    int objectid = doc.RootElement[0].GetProperty("Objectid").GetInt32();
                    return objectid;
                }
                else
                {
                    var contents = response.Content.ReadAsStringAsync();
                    return -1;
                }

            }


        }


    }
}