using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Models.DTO;
using ISHealthMonitor.Core.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;

namespace ISHealthMonitor.UI.Controllers.API
{
	[Route("api/[controller]")]
	[ApiController]
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public class RemindersController : ControllerBase
	{
		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;
		private readonly ILogger<RemindersController> _logger;

		public RemindersController(ILogger<RemindersController> logger,
			IHealthModel healthModel, IEmployee employee)
		{
			_logger = logger;
			_healthModel = healthModel;
			_employee = employee;

		}

		[HttpGet("GetReminders")]
		public string GetReminders()
		{
			var reminders = _healthModel.GetReminders();

			var retList = reminders.Select(r => new
			{
				r.ID,
				r.UserName,
				r.ISHealthMonitorSiteID,
				r.ISHealthMonitorIntervalID,
				r.ISHealthMonitorGroupSubmissionID,
				r.Action,
				r.CreatedBy,
				r.Site,
				r.ReminderInterval,
				SiteName = $"{r.Site?.SiteName} (ID={r.ISHealthMonitorSiteID})",
				IntervalName = $"{r.ReminderInterval?.DisplayName} (ID={r.ISHealthMonitorIntervalID})"
			}).ToList();

			return JsonConvert.SerializeObject(retList);
		}

		[HttpPut]
		[Route("DeleteReminder")]
		public IActionResult DeleteReminder(int id)
		{
			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");
			var employee = _employee.GetEmployeeByUserName(username);


			_healthModel.DeleteReminder(id);
            _logger.LogInformation($"Reminder ID={id.ToString()} deleted by {employee.GUID}");
            return Ok(id);
		}


		[HttpPut]
		[Route("DeleteReminderGroup")]
		public IActionResult DeleteReminderGroup(int groupId)
		{
			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");
			var employee = _employee.GetEmployeeByUserName(username);

			_healthModel.DeleteReminderGroup(groupId);
            _logger.LogInformation($"Reminder Group groupID={groupId.ToString()} deleted by {employee.GUID}");
            return Ok(groupId);
		}

		[HttpPut]
		[Route("DeleteRemindersForSite")]
		public IActionResult DeleteRemindersForSite(int siteId)
		{
			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");
			var employee = _employee.GetEmployeeByUserName(username);


			_healthModel.DeleteRemindersBySite(new Guid(employee.GUID), siteId);
            _logger.LogInformation("Reminders for Site siteID=" + siteId.ToString() + " deleted by" + employee.GUID);
            return Ok(siteId);
		}



		[HttpPost]
		[Route("CreateReminder")]
		public IActionResult CreateReminder([FromBody] UserReminderDTO reminderDTO)
		{

			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");


			var employee = _employee.GetEmployeeByUserName(username);


			if (reminderDTO.ID == 0)
			{
				var newReminder = new ISHealthMonitorUserReminderDbSet()
				{
					UserName = username,
					ISHealthMonitorSiteID = reminderDTO.ISHealthMonitorSiteID,
					ISHealthMonitorIntervalID = reminderDTO.ISHealthMonitorIntervalID,
					ISHealthMonitorGroupSubmissionID = reminderDTO.ISHealthMonitorGroupSubmissionID,
					Active = true,
					CreatedBy = new Guid(employee.GUID),
					CreatedDate = DateTime.Now,
					Deleted = false
				};

				_healthModel.AddReminder(newReminder);
				_logger.LogInformation($"Reminder created by {employee.GUID}: (siteID={reminderDTO.ISHealthMonitorSiteID} intervalID={reminderDTO.ISHealthMonitorIntervalID} groupID={reminderDTO.ISHealthMonitorGroupSubmissionID})");
				return Ok(newReminder);
			}
			else
			{
				var existingReminder = _healthModel.GetReminder(reminderDTO.ID);
				if (existingReminder != null)
				{

					existingReminder.ISHealthMonitorSiteID = reminderDTO.ISHealthMonitorSiteID;
					existingReminder.ISHealthMonitorIntervalID = reminderDTO.ISHealthMonitorIntervalID;
					existingReminder.ISHealthMonitorGroupSubmissionID = reminderDTO.ISHealthMonitorGroupSubmissionID;
					

					_healthModel.UpdateReminder(existingReminder);

                    _logger.LogInformation($"Reminder ID={reminderDTO.ID} updated by {employee.GUID} to: (siteID={reminderDTO.ISHealthMonitorSiteID} intervalID={reminderDTO.ISHealthMonitorIntervalID} groupID={reminderDTO.ISHealthMonitorGroupSubmissionID})");
                    return Ok(existingReminder);
				}
				else
				{
					return NotFound();
				}
			}
		}
		[HttpPost]
		[Route("CreateConfiguration")]
		public IActionResult CreateConfiguration([FromBody] ReminderConfiguration reminderConfiguration)
		{
			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var employee = _employee.GetEmployeeByUserName(username);

			int nextGroupID = _healthModel.GetNextReminderGroupID();

			List<ISHealthMonitorUserReminderDbSet> result = new List<ISHealthMonitorUserReminderDbSet> { };

			foreach (var reminder in reminderConfiguration.UserReminders)
			{
				ISHealthMonitorUserReminderDbSet newReminderDbSet = new ISHealthMonitorUserReminderDbSet
				{
					UserName = username,
					ISHealthMonitorSiteID = reminder.Site.ID,
					ISHealthMonitorIntervalID = reminder.ReminderInterval.ID,
					ISHealthMonitorGroupSubmissionID = nextGroupID,
					Active = true,
					CreatedBy = new Guid(employee.GUID),
					CreatedDate = DateTime.Now,
					Deleted = false
				};

				result.Add(newReminderDbSet);
			}

			foreach (var reminderDbSet in result)
			{
				_healthModel.AddReminder(reminderDbSet);
			}

			_logger.LogInformation($"Reminder Group created by {employee.GUID}: (groupID={nextGroupID.ToString()})");

			// Delete old group
			int prevGroupID = reminderConfiguration.GroupID;

			_healthModel.DeleteReminderGroup(prevGroupID);

            _logger.LogInformation($"Reminder Group automatically deleted by {employee.GUID}: (groupID={prevGroupID.ToString()})");


            return Ok(reminderConfiguration);
		}


		[HttpGet("GetReminderConfigurationData")]
		public async Task<IActionResult> GetReminderConfigurationData(int siteID)
		{
			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var employee = _employee.GetEmployeeByUserName(username);

			List<ReminderConfigurationData> reminderConfigurationDataList = _healthModel.GetReminderConfigurationsData(new Guid(employee.GUID), siteID);

			return Ok(JsonConvert.SerializeObject(reminderConfigurationDataList));


		}


	}
}
