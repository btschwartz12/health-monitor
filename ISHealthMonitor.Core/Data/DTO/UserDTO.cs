using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DTO
{
	public class UserDTO
	{
		public int ID { get; set; }
		public string Guid { get; set; }

        [Display(Name = "Has Admin Privileges")]
        public bool IsAdmin { get; set; }
        public string DisplayName { get; set; }
		public string Action { get; set; }
	}
}
