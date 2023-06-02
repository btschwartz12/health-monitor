using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DbSet
{
	public class ISHealthMonitorUserDbSet
	{
		public int ID { get; set; }
		public Guid Guid { get; set; }
		public bool IsAdmin { get; set; }
		public string DisplayName { get; set; }
		public bool Disabled { get; set; }
		public bool Deleted { get; set; }
		public DateTime DateCreated { get; set; }
	}
}
