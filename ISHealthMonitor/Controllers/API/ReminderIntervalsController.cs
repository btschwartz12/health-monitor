using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Implementations;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ISHealthMonitor.UI.Controllers.API
{
	[Route("api/[controller]")]
	[ApiController]
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]

    public class ReminderIntervalsController : ControllerBase
	{
		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;
		private readonly ILogger<ReminderIntervalsController> _logger;

		public ReminderIntervalsController(ILogger<ReminderIntervalsController> logger,
			IHealthModel healthModel,
			IEmployee employee)
		{
			_logger = logger;
			_healthModel = healthModel;
			_employee = employee;
		}

		[HttpGet("GetReminderIntervals")]
		public string GetReminderIntervals()
		{
			var retList = _healthModel.GetReminderIntervals();
			return JsonConvert.SerializeObject(retList);
		}

		[HttpPut]
		[Route("DeleteReminderInterval")]
		public IActionResult DeleteReminderInterval(int id)
		{
			List<string> subscribedUsers = _healthModel.GetSubscribedUsersForInterval(id);

			if (subscribedUsers.Count > 0)
			{
				return BadRequest(new { SubscribedUsers = subscribedUsers });
			}
			else
			{
				_healthModel.DeleteReminderInterval(id);
				return Ok(id);
			}
		}



		[HttpPost]
		[Route("CreateReminderInterval")]
		public IActionResult CreateReminderInterval([FromBody] ReminderIntervalDTO reminderIntervalDTO)
		{

			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");
			var employee = _employee.GetEmployeeByUserName(username);


			if (reminderIntervalDTO.ID == 0)
			{
				var newReminderInterval = new ISHealthMonitorIntervalDbSet()
				{
					DurationInMinutes = reminderIntervalDTO.DurationInMinutes,
					Type = reminderIntervalDTO.Type,
					DisplayName = reminderIntervalDTO.DisplayName,
					Active = true,
					CreatedBy = new Guid(employee.GUID),
					CreatedDate = DateTime.Now,
					Deleted = false,
					Disabled = false,

				};

				_healthModel.AddReminderInterval(newReminderInterval);
				return Ok(newReminderInterval);
			}
			else
			{
				var existingReminderInterval = _healthModel.GetReminderInterval(reminderIntervalDTO.ID);
				if (existingReminderInterval != null)
				{

					existingReminderInterval.DurationInMinutes = reminderIntervalDTO.DurationInMinutes;
					existingReminderInterval.Type = reminderIntervalDTO.Type;
					existingReminderInterval.DisplayName = reminderIntervalDTO.DisplayName;

					_healthModel.UpdateReminderInterval(existingReminderInterval);
					return Ok(existingReminderInterval);
				}
				else
				{
					return NotFound();
				}
			}
		}


        [HttpGet]
        [Route("GetIntervalsToSelect")]
        public async Task<IActionResult> GetSitesToSelect()
        {
			List<ReminderIntervalDTO> retList = _healthModel.GetReminderIntervals();

            var modifiedList = retList.Select(s => new
            {
                s.ID,
                s.DisplayName,
            }).ToList();

			

            return Ok(modifiedList);
        }
    }
}