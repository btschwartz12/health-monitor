using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Common
{
    public class WebServiceSettings
    {
        public string? ClientID { get; set; }
        public string? ClientSecret { get; set; }
        public string? TenantID { get; set; }

        public WebServiceSettings(
            string? clientid,
            string? clientsecret,
            string? tenantid)
        {
            ClientID = clientid;
            ClientSecret = clientsecret;
            TenantID = tenantid;
        }
    }
}
