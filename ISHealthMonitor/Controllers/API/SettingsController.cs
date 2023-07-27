using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ISHealthMonitor.UI.Controllers.API
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
	public class SettingsController : ControllerBase
	{
		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;
		private readonly ILogger<SettingsController> _logger;

		public SettingsController(ILogger<SettingsController> logger,
			IHealthModel healthModel,
			IEmployee employee)
		{
			_logger = logger;
			_healthModel = healthModel;
			_employee = employee;
		}

		[HttpGet("GetSettings")]
		public string GetSettings()
		{
			List<SettingDTO> retList = _healthModel.GetSettings();
			return JsonConvert.SerializeObject(retList);
		}

		[HttpPut]
		[Route("DeleteSetting")]
		public IActionResult DeleteSetting(int id)
		{
			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");
			var employee = _employee.GetEmployeeByUserName(username);

			_healthModel.DeleteSetting(id);
			_logger.LogInformation($"Setting ID={id.ToString()} deleted by {employee.GUID}");
			return Ok(id);
		}

		[HttpPost]
		[Route("CreateSetting")]
		public IActionResult CreateSetting([FromBody] SettingDTO settingDTO)
		{
			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");
			var employee = _employee.GetEmployeeByUserName(username);

			if (settingDTO.ID == 0)
			{
				var newSetting = new ISHealthMonitorSettingDbSet()
				{
					Name = settingDTO.Name,
					DisplayName = settingDTO.DisplayName,
					Value = settingDTO.Value,
					CreatedBy = new Guid(employee.GUID),
					CreatedDate = DateTime.Now,
					ModifiedBy = new Guid(employee.GUID),
					ModifiedDate = DateTime.Now,
					Deleted = false,
				};

				_healthModel.AddSetting(newSetting);

				_logger.LogInformation($"Setting created by {employee.GUID}: ({settingDTO.DisplayName} = {settingDTO.Value})");

				return Ok(newSetting);
			}
			else
			{
				var existingSetting = _healthModel.GetSetting(settingDTO.ID);
				if (existingSetting != null)
				{
					existingSetting.Name = settingDTO.Name;
					existingSetting.DisplayName = settingDTO.DisplayName;
					existingSetting.Value = settingDTO.Value;
					existingSetting.ModifiedBy = new Guid(employee.GUID);
					existingSetting.ModifiedDate = DateTime.Now;

					_healthModel.UpdateSetting(existingSetting);

					_logger.LogInformation($"Setting ID={settingDTO.ID} updated by {employee.GUID} to: ({settingDTO.Name} = {settingDTO.Value}) and possibly other fields");
					return Ok(existingSetting);
				}
				else
				{
					return NotFound();
				}
			}
		}
	}

}
