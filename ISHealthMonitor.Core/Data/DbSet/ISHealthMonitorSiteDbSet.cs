using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DbSet
{
	public class ISHealthMonitorSiteDbSet
	{
		public int ID { get; set; }
		public string URL { get; set; }
		public string DisplayName { get; set; }
		public DateTime SSLEffectiveDate { get; set; }
		public DateTime SSLExpirationDate { get; set; }
		public string SSLIssuer { get; set; }
		public string SSLSubject { get; set; }
		public string SSLCommonName { get; set; }
		public string SSLThumbprint { get; set; }
		public string SiteCategory { get; set; }
		public Guid CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime LastUpdated { get; set; }
		public bool Active { get; set; }
		public bool Deleted { get; set; }
		public bool Disabled { get; set; }
	}

}


