using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ISHealthMonitor.UI.ViewModels;
using ISHealthMonitor.Core.DataAccess;
using ISHealthMonitor.Core.Implementations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.IO;
using ISHealthMonitor.Core.Model;
using ISHealthMonitor.Core.Data.DTO;
using System.Security.Policy;
using System.Security.Claims;

namespace ISHealthMonitor.Controllers
{
    [Authorize]
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

            var username = HttpContext.User.Identity.Name;


            var CurrentEmployee = _employee.GetEmployeeByEmail(username);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

            bool userHasReminders = await _healthModel.UserHasReminders(new Guid(CurrentEmployee.GUID));
            ViewBag.UserHasReminders = userHasReminders;


            _logger.LogInformation("Home Page Visitor: " + CurrentEmployee.DisplayName + " (has reminders: " + userHasReminders.ToString() + ")");

            


            HomeViewModel model = new()
            {
                Username = username,
                DisplayName = CurrentEmployee.DisplayName,
            };


            return View(model);
        }



        public async Task<IActionResult> LogViewer()
        {

            var username = HttpContext.User.Identity.Name;

            var CurrentEmployee = _employee.GetEmployeeByEmail(username);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

            


            LogViewerModel model = new LogViewerModel()
            {
                Today = DateTime.Now.ToShortDateString(),
                LastWeek = DateTime.Now.AddDays(-7).ToShortDateString(),
                LogFiles = new List<LogFile>() { }, // Api will fill this in
            };

            return View("~/Views/Admin/LogViewer.cshtml", model);
        }


        public async Task<IActionResult> WorkOrderBuilder(int siteId = 0)
        {
            var username = HttpContext.User.Identity.Name;

            var CurrentEmployee = _employee.GetEmployeeByEmail(username);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

            
            try
            {

                if (siteId == 0)
                {
                    throw new Exception("No siteId provided as a query parameter");
                }

                SiteDTO site = _healthModel.GetSiteDTO(siteId);

                WorkOrderDTO model = new WorkOrderDTO()
                {
                    SiteID = siteId,
                    SiteName = site.SiteName,
                    SiteURL = site.SiteURL,
                    IssueType = "Other",
                    Category = "Help Desk",
                    System = "Help Desk",
					ShortDescription = $"Update certificate for {site.SiteName}",
					Urgency = "2",
					Description = $"The SSL Certificate is going to expire on: {site.SSLExpirationDate} for the site URL: {site.SiteURL}"
				};
                return View(model);
            }
            catch (Exception ex) 
            {

                return BadRequest(ex.Message);
            }
            
        }





    }
}

