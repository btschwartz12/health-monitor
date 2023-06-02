using ISHealthMonitor.Core.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.Models
{
    public class SiteReminderConfiguration
    {
        public int ISHealthMonitorSiteID { get; set; }
        public string SiteName { get; set; }
        public List<ReminderIntervalDTO> ReminderIntervals { get; set; }
        public string Action { get; set; }


		//public Dictionary<string, bool> ReminderIntervals { get; set; }
		//
	}
}
