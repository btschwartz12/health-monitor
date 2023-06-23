using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Models;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ISHealthMonitor.Core.Models;
using ISHealthMonitor.UI.ViewModels;
using ISHealthMonitor.Core.Helpers;
using ISHealthMonitor.Core.Common;
using ISHealthMonitor.Core.DataAccess;
using ISHealthMonitor.Core.Implementations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace ISHealthMonitor.Controllers
{
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;
        private readonly IRest _restModel;
        private readonly IConfiguration _config;
        public HomeController(ILogger<HomeController> logger,
            IHealthModel healthModel, IEmployee employee, IRest restModel, IConfiguration config)
        {
            _logger = logger;
            _employee = employee;
            _healthModel = healthModel;
            _restModel = restModel;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {

            var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var CurrentEmployee = _employee.GetEmployeeByUserName(user);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

			bool userHasReminders = await _healthModel.UserHasReminders(new Guid(CurrentEmployee.GUID));
			ViewBag.UserHasReminders = userHasReminders;



			HomeViewModel model = new()
            {
                Username = user,
                DisplayName = CurrentEmployee.DisplayName,
            };


            return View(model);
        }





    }
}
