using ISHealthMonitor.Core.Common;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.DataAccess
{
    public class ConfluenceCloudAccess
    {
        private static ConfluenceAPI? _confluenceAPISettings;
        public ConfluenceCloudAccess(ConfluenceAPI confluenceAPISettings)
        {
            _confluenceAPISettings = confluenceAPISettings;

        }
        public static string GetExistingJiraTicket(int pcrId)
        {
            try
            {
                string jqlQuery = $"'PCR Id'~{pcrId}&maxResults=1&fields=null";
                
                string url = _confluenceAPISettings.Endpoint + $"/rest/api/2/search?jql={jqlQuery}";
                string? jiraUser = _confluenceAPISettings.ServiceAccount;
                string? jiraPw = _confluenceAPISettings.APIToken;

                // Begin making POST request to Jira
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    client.Headers.Add(HttpRequestHeader.Accept, "*/*");
                    client.Headers.Add(HttpRequestHeader.Authorization, "Basic " + CommonFunctions.Base64Encode($"{jiraUser}:{jiraPw}"));


                    string response = client.DownloadString(new Uri(url));

                    //var jiraResult = JsonConvert.DeserializeObject<JiraIssue>(response);
                    //var customfield_10602 = "";
                    //var customfield_11530 = "";
                    //if (jiraResult != null)
                    //{
                    //    customfield_10602 = jiraResult.fields["customfield_10602"];
                    //    customfield_11530 = jiraResult.fields["customfield_11530"];
                    //}

                }
            }

            catch (Exception ex)
            {
            }

            return null;
        }
        public static string UpdateTicketFields(string baseURL, string jiraKey, List<string> fieldList)
        {

            try
            {
                string? url = _confluenceAPISettings.Endpoint + @"/rest/api/2/issue/" + jiraKey;
                string? jiraUser = _confluenceAPISettings.ServiceAccount;
                string? jiraPw = _confluenceAPISettings.APIToken;

                string json = "{\"fields\":{" + string.Join(",", fieldList) + "}}";



                // Begin making POST request to Jira
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    client.Headers.Add(HttpRequestHeader.Accept, "*/*");
                    client.Headers.Add(HttpRequestHeader.Authorization, "Basic " + CommonFunctions.Base64Encode($"{jiraUser}:{jiraPw}"));

                    string response = client.UploadString(new Uri(url), "PUT", json);

                    return "true";
                }
            }

            catch (Exception ex)
            {
                if (ex.Message != "The remote server returned an error: (404) Not Found.")
                {
                    return "404";
                }
                else
                {
                    return "error";

                }
                //throw ex;
            }


        }

    }
}
