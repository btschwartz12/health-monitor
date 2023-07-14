using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISHealthMonitor.Core.DataAccess
{
    public class AuthToken
    {
        public AuthToken()
        {

        }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}