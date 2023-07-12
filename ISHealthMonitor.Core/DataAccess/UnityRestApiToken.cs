using ISHealthMonitor.Core.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace ISHealthMonitor.Core.DataAccess
{
    public class UnityRestAPIToken
    {
        private readonly ILogger<object> _logger;
        private static string unityRestApiTokenParmUserName;
        private static string unityRestApiTokenParmPassword;
        private static string unityRestApiTokenURL;
        private readonly IRest _restModel;

        private readonly IConfiguration _config;

        public AuthToken AuthenticationToken;

        //public UnityRestAPIToken(string unityUser, string unityPswd, string unityTokenUrl)
        //{
        //    unityRestApiTokenParmUserName = unityUser;
           
        //    unityRestApiTokenParmPassword = unityPswd;
        //    unityRestApiTokenURL = unityTokenUrl;
        //    GetBearerToken();
        //}

        public UnityRestAPIToken(ILogger<object> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            unityRestApiTokenParmUserName = _config.GetSection("UnityRestAPI")["unityRestApiTokenParmUserName"];
            unityRestApiTokenParmPassword = _config.GetSection("UnityRestAPI")["unityRestApiTokenParmPassword"];
            unityRestApiTokenURL = _config.GetSection("UnityRestAPI")["unityRestApiTokenURL"];
            GetBearerToken();
        }

        private void GetBearerToken()
        {
            _logger.LogInformation("unityRestApiTokenURL: " + unityRestApiTokenURL);
            try
            {
                using (var client = new HttpClient())
                {
                    var tokenParams = "grant_type=password&username=" + unityRestApiTokenParmUserName + "&password=" + unityRestApiTokenParmPassword;
                    var tokenURL = unityRestApiTokenURL + tokenParams;
                    var content = new StringContent(tokenParams, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(new Uri(tokenURL), content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var contents = response.Content.ReadAsStringAsync();

                        AuthenticationToken = JsonConvert.DeserializeObject<AuthToken>(contents.Result);
                    }
                }
            }
            catch (Exception ex)
            {
                var logBody = "<br><br><div> <b>Exception Message:</b> " + ex.Message.ToString() + " </div><br><br><div> <b>Inner Exception:</b> " + ex.InnerException.ToString() + " </div>";

                _logger.LogError(logBody, "SalesforceHelper AuthenticateWithSalesforce Method");
                AuthenticationToken.AccessToken = "Failed to Authenticate";
            }
            
        }

    }
}