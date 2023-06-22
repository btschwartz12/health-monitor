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
			var retList = _healthModel.GetReminders();
			return JsonConvert.SerializeObject(retList);
		}

		[HttpPut]
		[Route("DeleteReminder")]
		public IActionResult DeleteReminder(int id)
		{
			_healthModel.DeleteReminder(id);
			return Ok(id);
		}


		[HttpPut]
		[Route("DeleteReminderGroup")]
		public IActionResult DeleteReminderGroup(int groupId)
		{
			_healthModel.DeleteReminderGroup(groupId);
			return Ok(groupId);
		}

		[HttpPut]
		[Route("DeleteRemindersForSite")]
		public IActionResult DeleteRemindersForSite(int siteId)
		{
			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");
			var employee = _employee.GetEmployeeByUserName(username);


			_healthModel.DeleteRemindersBySite(new Guid(employee.GUID), siteId);
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

			// Delete old group
			int prevGroupID = reminderConfiguration.GroupID;

			_healthModel.DeleteReminderGroup(prevGroupID);


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
