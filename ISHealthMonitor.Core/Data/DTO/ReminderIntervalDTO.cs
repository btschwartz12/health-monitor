using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ISHealthMonitor.Core.Data.DTO
{
	public class ReminderIntervalDTO
	{
		public int ID { get; set; }

		[Required(ErrorMessage = "DurationInMinutes is required")]
		[Display(Name = "Duration (Minutes)")]
		public int DurationInMinutes { get; set; }

		[Required(ErrorMessage = "Type is required")]
		[Display(Name = "Type")]
		public string? Type { get; set; }

		[Required(ErrorMessage = "DisplayName is required")]
		[Display(Name = "Display Name")]
		public string? DisplayName { get; set; }

		[Display(Name = "Action")]
		public string? Action { get; set; }
	}

}
