using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DTO
{
	public class UserReminderDTO
	{
		public int ID { get; set; }
		public string? UserName { get; set; }
		
		[Required(ErrorMessage = "ISHealthMonitorSiteID is required")]
		[Display(Name = "IS Health Monitor Site ID")]
		public int ISHealthMonitorSiteID { get; set; }

		[Required(ErrorMessage = "ISHealthMonitorIntervalID is required")]
		[Display(Name = "IS Health Monitor Interval ID")]
		public int ISHealthMonitorIntervalID { get; set; }

		[Required(ErrorMessage = "ISHealthMonitorGroupSubmissionID is required")]
		[Display(Name = "IS Health Monitor Group Submission ID")]
		public int ISHealthMonitorGroupSubmissionID { get; set; }
		public string? Action { get; set; }


		public Guid? CreatedBy { get; set; }


		public SiteDTO? Site { get; set; }
		public ReminderIntervalDTO? ReminderInterval { get; set; }
	}
}
