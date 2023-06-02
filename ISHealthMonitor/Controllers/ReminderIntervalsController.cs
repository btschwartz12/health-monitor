using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ISHealthMonitor.UI.Controllers
{
    [Authorize(Policy = "Admin")]
    public class ReminderIntervalsController : Controller
	{

		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;

        public ReminderIntervalsController(IHealthModel healthModel, IEmployee employee)
		{
			_healthModel = healthModel;
			_employee = employee;
		}
		public IActionResult Index()
		{
            var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

            var CurrentEmployee = _employee.GetEmployeeByUserName(user);

            ViewBag.UserName = user;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

            return View();
		}

		[HttpGet]
		public IActionResult AddEdit(int id = 0)
		{
			if (id == 0)
			{
				ReminderIntervalDTO newReminderIntervalDTO = new()
				{

				};
				return View(newReminderIntervalDTO);
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

				return View(reminderIntervalDTO);
			}
		}
	}
}
