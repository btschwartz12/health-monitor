using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace ISHealthMonitor.UI.Controllers
{
    //[Authorize(Policy = "Admin", AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public class SettingsController : Controller
	{
		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;
		private readonly IConfiguration _config;

		public SettingsController(IHealthModel healthModel, IEmployee employee, IConfiguration config)
		{
			_healthModel = healthModel;
			_employee = employee;
			_config = config;
		}

		public IActionResult Index()
		{
			var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");
			var CurrentEmployee = _employee.GetEmployeeByUserName(user);

			ViewBag.UserName = CurrentEmployee.DisplayName;
			ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

			return View("~/Views/Admin/Settings/Index.cshtml");
		}

		[HttpGet]
		public IActionResult AddEdit(int id = 0)
		{
			if (id == 0)
			{
				SettingDTO newSettingDTO = new()
				{

				};
				return View("~/Views/Admin/Settings/AddEdit.cshtml", newSettingDTO);
			}
			else
			{
				ISHealthMonitorSettingDbSet setting = _healthModel.GetSetting(id);

				SettingDTO settingDTO = new()
				{
					ID = setting.ID,
					Name = setting.Name,
					DisplayName = setting.DisplayName,
					Value = setting.Value,
				};

				return View("~/Views/Admin/Settings/AddEdit.cshtml", settingDTO);
			}
		}
	}

}
