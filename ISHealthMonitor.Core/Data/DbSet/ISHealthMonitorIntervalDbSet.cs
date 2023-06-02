using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DbSet
{
	public class ISHealthMonitorIntervalDbSet
	{
		public int ID { get; set; }
		public int DurationInMinutes { get; set; }
		public string Type { get; set; }
		public string DisplayName { get; set; }
		public bool Active { get; set; }
		public Guid CreatedBy { get; set; }
		public DateTime CreatedDate { get; set; }
		public bool Deleted { get; set; }
		public bool Disabled { get; set; }
	}

}

