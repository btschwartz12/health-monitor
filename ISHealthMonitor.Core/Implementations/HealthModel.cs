using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.Contexts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Models
{
    public class HealthModel : IHealthModel
    {
        private readonly IACMSEntityContext _IACMSEntityContext;

        public HealthModel(IACMSEntityContext context)
        {
            _IACMSEntityContext = context;
        }

        public List<CertificateDTO> GetSites()
        {
            var sites = _IACMSEntityContext.Site
             .Where(x => x.Active == true).ToList();

           return (from d in sites
									   select new CertificateDTO()
                                       {
                                            SiteName =d.DisplayName,
                                            id = d.ID,
                                            SiteURL =d.DisplayName,
  										    Action = "<a href='#' onclick=DATATABLE_REQUESTS.EditRecord(" + d.ID + ")  title = 'Edit' class='padding2 Edit' ><i class='fa fa-pencil-square-o fa-2x' aria-hidden='true'></i></a>" +
											"<a href='#' class='padding2 Delete' onclick=DATATABLE_REQUESTS.DeleteRecord(" + d.ID + ") title = 'Delete' ><i class='fa fa-trash-o fa-2x' aria-hidden='true'></i></a>"


									   }).ToList();


										   


        }
		public List<CertificateDTO> GetSitesSSLInfo()
		{
			var certificateDTOList = new List<CertificateDTO>();

			var sites = _IACMSEntityContext.Site
			 .Where(x => x.Active == true).ToList();


			var t = GetsAsync();

			return certificateDTOList;
		}
		private async Task<CertificateDTO> GetsAsync()
        {
            var certHandlers = new CertificateHandlers();
            return await certHandlers.CheckAzureSiteAsync("https://hub.hyland.com/");
        }

    }
}
