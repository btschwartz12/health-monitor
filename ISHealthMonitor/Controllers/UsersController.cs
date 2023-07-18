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
    [Authorize(Policy = "Admin")]
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public class UsersController : Controller
	{

		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;
		private readonly IConfiguration _config;

		public UsersController(IHealthModel healthModel, IEmployee employee, IConfiguration config)
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

            

            return View("~/Views/Admin/Users/Index.cshtml");
		}

		[HttpGet]
		public IActionResult AddEdit(int id = 0)
		{
			if (id == 0)
			{

				UserDTO newUserDTO = new()
				{
					Guid = "",
					IsAdmin = false
				};
				return View("~/Views/Admin/Users/AddEdit.cshtml", newUserDTO);
			}
			else
			{
				ISHealthMonitorUserDbSet user = _healthModel.GetUser(id);


				UserDTO userDTO = new()
				{
					ID = user.ID,
					Guid = user.Guid.ToString(),
					IsAdmin = user.IsAdmin,
					DisplayName = user.DisplayName
				};

				return View("~/Views/Admin/Users/AddEdit.cshtml", userDTO);
			}
		}
	}
}
