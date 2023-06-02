using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.Models
{
    public class ReminderConfigurationData
    {
        public int GroupID { get; set; }
        public int NumReminders { get; set; }
        public string CreatedDate { get; set; }
        public List<string> ConfiguredSiteNames { get; set; }
        public string Action { get; set; }

		// Additional Dictionary for tooltips
		public Dictionary<string, List<string>> SiteToRemindersMap { get; set; }
	}
}
