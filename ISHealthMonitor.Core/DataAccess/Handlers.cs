using ISHealthMonitor.Core.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Models
{
    public class CertificateHandlers
    {
        private HttpClientHandler _handler;
        private HttpClient _client;
        public async Task<CertificateDTO> CheckAzureSiteAsync(string siteURL)
        {
            var retModel = new CertificateDTO();

            _handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) =>
                {
                    //check to make sure message is the correct URL
                    //Our external sites returns our Azure login https://login.microsoftonline.com/8ca5db88-a5ab-48f7-a5e0-4ce50935f807/wsfed?
                    if (message.RequestUri.ToString().Contains(siteURL))
                    {
                        retModel.EffectiveDate = certificate.GetEffectiveDateString();
                        retModel.ExpDate = certificate.GetExpirationDateString();
                        retModel.Issuer = certificate.Issuer;
                        retModel.Subject = certificate.Subject;
                    }
                    return true;
                }
            };

            // Create an HttpClient object
            using (_client = new HttpClient(_handler))
            {
                using (var response = new HttpResponseMessage())
                {
                    await _client.GetAsync(siteURL);
                }
            }
            return retModel;

        }
                
    }
   
}
