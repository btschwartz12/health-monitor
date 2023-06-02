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

namespace ISHealthMonitor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;
        public HomeController(ILogger<HomeController> logger,
            IHealthModel healthModel, IEmployee employee)
        {
            _logger = logger;
            _employee = employee;
            _healthModel = healthModel;

        }

        public IActionResult Index()
        {
            
            var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var CurrentEmployee = _employee.GetEmployeeByUserName(user);

			ViewBag.UserName = user;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));



            



			HomeViewModel model = new()
            {
                Username = user,
                DisplayName = CurrentEmployee.DisplayName,
            };


            return View(model);
        }
		
		

        


     

      
    }
}
