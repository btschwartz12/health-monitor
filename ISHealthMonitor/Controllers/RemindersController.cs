using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Data.Models;
using ISHealthMonitor.Core.Implementations;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ISHealthMonitor.UI.Controllers
{
    [Authorize]
    public class RemindersController : Controller
	{

		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;
		private readonly IConfiguration _config;

        public RemindersController(IHealthModel healthModel, IEmployee employee, IConfiguration config)
        {
            _healthModel = healthModel;
			_employee = employee;
			_config = config;
        }
		[Authorize(Policy = "Admin")]
		public IActionResult Index()
		{
            var username = HttpContext.User.Identity.Name;

            var CurrentEmployee = _employee.GetEmployeeByEmail(username);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

           

            return View("~/Views/Admin/Reminders/Index.cshtml");
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
				return View("~/Views/Admin/Reminders/AddEdit.cshtml", newReminderDTO);
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

				return View("~/Views/Admin/Reminders/AddEdit.cshtml", reminderDTO);
			}
		}

		public IActionResult ConfigurationHistory(int siteID = 0)
		{
            var username = HttpContext.User.Identity.Name;

            var CurrentEmployee = _employee.GetEmployeeByEmail(username);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));
            ViewBag.SiteDTO = _healthModel.GetSiteDTO(siteID);


            return View("~/Views/Home/ConfigurationHistory.cshtml");
		}

		public IActionResult ConfigurationBuilder(int groupID = 0, int siteID = 0)
        {

			var username = HttpContext.User.Identity.Name;

			var CurrentEmployee = _employee.GetEmployeeByEmail(username);

			ViewBag.UserName = CurrentEmployee.DisplayName;
			ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

            

            if (groupID == 0)
			{
				if (siteID == 0)
				{
					ReminderConfiguration viewModel = new ReminderConfiguration()
					{
						GroupID = 0,
						UserReminders = new List<UserReminderDTO> { },
					};
					return View("~/Views/Home/ConfigurationBuilder.cshtml", viewModel);
				}
				else
				{

					int intervalID = _healthModel.GetReminderIntervals()[0].ID;

					var reminder = new UserReminderDTO()
					{
						ID = 0,
						UserName = "x",
						ISHealthMonitorSiteID = siteID,
						ISHealthMonitorIntervalID = intervalID,
						ISHealthMonitorGroupSubmissionID = 0,
						Action = "x",
						Site = _healthModel.GetSiteDTO(siteID),
						ReminderInterval = _healthModel.GetReminderIntervalDTO(intervalID)
					};

					ReminderConfiguration viewModel = new ReminderConfiguration()
					{
						GroupID = 0,
						UserReminders = new List<UserReminderDTO> { reminder },
					};
					return View("~/Views/Home/ConfigurationBuilder.cshtml", viewModel);

				}
				
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
					
					if (firstReminder.CreatedBy.ToString() != CurrentEmployee.GUID)
					{
						return RedirectToAction("Index", "Home");
					}
				}

                DateTime groupCreatedDate = _healthModel.GetCreatedDateForGroup(groupID);
                ViewBag.GroupUserName = remindersByGroup[0].UserName;

                ReminderConfiguration viewModel = new ReminderConfiguration()
			    {
					GroupID = groupID,
			        UserReminders = remindersByGroup,
					DateCreated = groupCreatedDate.ToString("dddd, MMMM d, yyyy, HH:mm:ss")
			};

			    return View("~/Views/Home/ConfigurationBuilder.cshtml", viewModel);
			}


        }
    }
}
