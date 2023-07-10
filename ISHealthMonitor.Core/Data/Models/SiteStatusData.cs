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
		public Dictionary<string, List<string>> UsersSubscribed { get; set; }
		// key is the users name, value is the list of intervals they have configured
	}
}
