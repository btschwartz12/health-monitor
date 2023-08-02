using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Implementations;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace ISHealthMonitor.UI.Controllers
{
    [Authorize(Policy = "Admin")]
    public class ReminderIntervalsController : Controller
	{

		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;
        private readonly IConfiguration _config;

        public ReminderIntervalsController(IHealthModel healthModel, IEmployee employee, IConfiguration config)
		{
			_healthModel = healthModel;
			_employee = employee;
			_config = config;
		}
		public IActionResult Index()
		{
            var username = HttpContext.User.Identity.Name;

            var CurrentEmployee = _employee.GetEmployeeByEmail(username);

			ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

            

            return View("~/Views/Admin/ReminderIntervals/Index.cshtml");
		}

		[HttpGet]
		public IActionResult AddEdit(int id = 0)
		{
			if (id == 0)
			{
				ReminderIntervalDTO newReminderIntervalDTO = new()
				{

				};
				return View("~/Views/Admin/ReminderIntervals/AddEdit.cshtml", newReminderIntervalDTO);
			}
			else
			{
				ISHealthMonitorIntervalDbSet reminderInterval = _healthModel.GetReminderInterval(id);

				ReminderIntervalDTO reminderIntervalDTO = new()
				{
					ID = reminderInterval.ID,
					DurationInMinutes = reminderInterval.DurationInMinutes,
					Type = reminderInterval.Type,
					DisplayName = reminderInterval.DisplayName,
				};

				return View("~/Views/Admin/ReminderIntervals/AddEdit.cshtml", reminderIntervalDTO);
			}
		}
	}
}
