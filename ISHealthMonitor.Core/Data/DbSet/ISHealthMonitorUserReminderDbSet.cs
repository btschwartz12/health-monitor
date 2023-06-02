using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DbSet
{
	public class ISHealthMonitorUserReminderDbSet
	{
		public int ID { get; set; }
		public string UserName { get; set; }
		public int ISHealthMonitorSiteID { get; set; }
		public int ISHealthMonitorIntervalID { get; set; }
		public int ISHealthMonitorGroupSubmissionID { get; set; }
		public bool Active { get; set; }
		public Guid CreatedBy { get; set; }
		public DateTime CreatedDate { get; set; }
		public bool Deleted { get; set; }


		public ISHealthMonitorSiteDbSet? ISHealthMonitorSite { get; set; }
		public ISHealthMonitorIntervalDbSet? ISHealthMonitorInterval { get; set; }
	}
}


/*
 *  [ID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [varchar](100) NOT NULL,
	[ISHealthMonitorSiteID] [int] NOT NULL,
	[ISHealthMonitorIntervalID] [int] NOT NULL,
	[ISHealthMonitorGroupSubmissionID] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 */