using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.Models
{
	public class SiteStatusData
	{
		public List<SiteStatus> SiteStatusList { get; set; }
	}

	public class SiteStatus
	{
		public int SiteID { get; set; }
		public string SiteName { get; set; }
		public string SiteURL { get; set; }
		public string SSLExpirationDate { get; set; }
		public string TimeUntilExpiration { get; set; }
		public int NumSubscribedUsers { get; set; }
		public string Action { get; set; }
		public string WorkOrderAction { get; set; }
		public string StatusIcon { get; set; }
		public string RowColor { get; set; }
	}
}
