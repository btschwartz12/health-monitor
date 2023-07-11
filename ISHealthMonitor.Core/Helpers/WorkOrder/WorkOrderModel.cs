using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Helpers.WorkOrder
{
	public class WorkOrderModel
	{
		[Required(ErrorMessage = "IssueType is required")]
		[Display(Name = "Issue Type")]
		public string IssueType { get; set; }

		[Required(ErrorMessage = "Category is required")]
		[Display(Name = "Category")]
		public string Category { get; set; }

		[Required(ErrorMessage = "System is required")]
		[Display(Name = "System")]
		public string System { get; set; }

		[Required(ErrorMessage = "Urgency is required")]
		[Display(Name = "Urgency")]
		public string Urgency { get; set; }

		[Required(ErrorMessage = "Short Description is required")]
		[Display(Name = "Short Description")]
		public string ShortDescription { get; set; }

		[Required(ErrorMessage = "Description is required")]
		[Display(Name = "Description")]
		public string Description { get; set; }

        [Required(ErrorMessage = "Emergency Reason is required")]
        [Display(Name = "Emergency Reason")]
        public string EmergencyReason { get; set; }
	}

}
