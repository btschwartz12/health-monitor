using ISHealthMonitor.Core.Data.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Helpers.Confluence
{
    public class ConfluenceTableModel
    {
        public List<ConfluenceSiteRowModel> sites;

		public readonly string TemplateRelativePath = "wwwroot\\lib\\templates\\ConfluenceTableTemplate.html";



		public Dictionary<string, string> Colors { get; set; }
		public Dictionary<string, int> Thresholds { get; set; }


    }

    public class ConfluenceSiteRowModel
    {
		public int ID { get; set; }
		public string? SiteURL { get; set; }
		public string? SiteName { get; set; }
		public string? SSLEffectiveDate { get; set; }
		public string? SSLExpirationDate { get; set; }
		public string? SSLIssuer { get; set; }
		public string? SSLSubject { get; set; }
		public string? SSLCommonName { get; set; }
		public string? SSLThumbprint { get; set; }
		public string? TimeUntilExpiration { get; set; }
		public string? RowColor { get; set; }
		public bool PendingWorkOrder { get; set; }
		public string? WorkOrderURL { get; set; }
		public string? WorkOrderSubmittedDate { get; set; }
		public string? Notes { get; set; }
		
	}

}
