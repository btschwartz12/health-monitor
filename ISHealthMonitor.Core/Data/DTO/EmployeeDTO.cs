using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DTO
{


public class EmployeeDTO

	{

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string DisplayName { get; set; }

		public string Department { get; set; }

		public string Company { get; set; }

		public string Manager { get; set; }

		public string Title { get; set; }

		public string Email { get; set; }

		public string Office { get; set; }

		[Key]

		public string GUID { get; set; }

		public string NetworkLogon { get; set; }

		public int empid { get; set; }


	}
}


