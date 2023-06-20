using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Data.Models;
using ISHealthMonitor.Core.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ISHealthMonitor.UI.Controllers
{
    
    public class RemindersController : Controller
	{

		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;

        public RemindersController(IHealthModel healthModel, IEmployee employee)
        {
            _healthModel = healthModel;
			_employee = employee;
        }
		[Authorize(Policy = "Admin")]
		public IActionResult Index()
		{
            var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

            var CurrentEmployee = _employee.GetEmployeeByUserName(user);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

            return View();
		}
		[Authorize(Policy = "Admin")]
		[HttpGet]
		public IActionResult AddEdit(int id = 0)
		{
			if (id == 0)
			{
				UserReminderDTO newReminderDTO = new()
				{
					
				};
				return View(newReminderDTO);
			}
			else
			{
				ISHealthMonitorUserReminderDbSet reminder = _healthModel.GetReminder(id);

				UserReminderDTO reminderDTO = new()
				{
					ID = reminder.ID,
					UserName = reminder.UserName,
					ISHealthMonitorSiteID = reminder.ISHealthMonitorSiteID,
					ISHealthMonitorIntervalID = reminder.ISHealthMonitorIntervalID,
					ISHealthMonitorGroupSubmissionID = reminder.ISHealthMonitorGroupSubmissionID
				};

				return View(reminderDTO);
			}
		}

		public IActionResult ConfigurationHistory(int siteID = 0)
		{
            var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

            var CurrentEmployee = _employee.GetEmployeeByUserName(user);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));
            ViewBag.SiteDTO = _healthModel.GetSiteDTO(siteID);
			return View();
		}

		public IActionResult ConfigurationBuilder(int groupID = 0)
        {

			var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var CurrentEmployee = _employee.GetEmployeeByUserName(user);

			ViewBag.UserName = CurrentEmployee.DisplayName;
			ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

			if (groupID == 0)
			{
				ReminderConfiguration viewModel = new ReminderConfiguration()
				{
					GroupID = 0,
			        UserReminders = new List<UserReminderDTO> { },
			    };
			    return View(viewModel);
			}
			else
			{
			    List<UserReminderDTO> remindersByGroup = _healthModel.GetReminders()
			    .Where(r => r.ISHealthMonitorGroupSubmissionID == groupID)
			    .ToList();

				if (remindersByGroup.Count == 0) 
				{
					return RedirectToAction("Index", "Home");
				}


				// Need to make sure correct user is accessing this
				if (!ViewBag.UserIsAdmin)
				{
					var firstReminder = remindersByGroup[0];
					// Should I be using GUID instead?
					if (firstReminder.UserName != user)
					{
						return RedirectToAction("Index", "Home");
					}
				}

			    ReminderConfiguration viewModel = new ReminderConfiguration()
			    {
					GroupID = groupID,
			        UserReminders = remindersByGroup
			    };

			    return View(viewModel);
			}


        }
    }
}
