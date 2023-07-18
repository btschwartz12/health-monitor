using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DbSet
{
	public class ISHealthMonitorSettingDbSet
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string Value { get; set; }
		public DateTime CreatedDate { get; set; }
		public Guid CreatedBy { get; set; }
		public DateTime ModifiedDate { get; set; }
		public Guid ModifiedBy { get; set; }
		public bool Deleted { get; set; }
	}
}
