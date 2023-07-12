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
using ISHealthMonitor.Core.Helpers.WorkOrder;
using ISHealthMonitor.Core.Model;
using ISHealthMonitor.Core.Data.DTO;
using System.Security.Policy;

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


            _logger.LogInformation("Home Page Visitor: " + CurrentEmployee.DisplayName + " (has reminders: " + userHasReminders.ToString() + ")");


            if (ViewBag.UserIsAdmin)
            {
                string username = _config.GetSection("ApiAuthConfig")["userName"];
                string password = _config.GetSection("ApiAuthConfig")["password"];

                ViewBag.ApiAuthUserName = username;
                ViewBag.ApiAuthPassword = password;
            }




            //var token = new UnityRestAPIToken(_logger, _config);

            //var f = token.AuthenticationToken;

            //var unity = new UnityRestAPIAccess(_logger, _config);

            //var email = "Nick.Susanjar@hyland.com";
            //var id = unity.GetRequestorId(email);



            HomeViewModel model = new()
            {
                Username = user,
                DisplayName = CurrentEmployee.DisplayName,
            };


            return View(model);
        }



        public async Task<IActionResult> LogViewer()
        {

            var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

            var CurrentEmployee = _employee.GetEmployeeByUserName(user);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

            if (ViewBag.UserIsAdmin)
            {
                string username = _config.GetSection("ApiAuthConfig")["userName"];
                string password = _config.GetSection("ApiAuthConfig")["password"];

                ViewBag.ApiAuthUserName = username;
                ViewBag.ApiAuthPassword = password;
            }


            LogViewerModel model = new LogViewerModel()
            {
                Today = DateTime.Now.ToShortDateString(),
                LastWeek = DateTime.Now.AddDays(-7).ToShortDateString(),
                LogFiles = new List<LogFile>() { }, // Api will fill this in
            };

            return View(model);
        }


        public async Task<IActionResult> WorkOrderBuilder(int siteId = 0)
        {
            var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

            var CurrentEmployee = _employee.GetEmployeeByUserName(user);

            ViewBag.UserName = CurrentEmployee.DisplayName;
            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

            if (ViewBag.UserIsAdmin)
            {
                string username = _config.GetSection("ApiAuthConfig")["userName"];
                string password = _config.GetSection("ApiAuthConfig")["password"];

                ViewBag.ApiAuthUserName = username;
                ViewBag.ApiAuthPassword = password;
            }


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
                    ShortDescription = $"Update certificate for {site.SiteName} ({site.SiteURL})",
                    Urgency = "2",
                    Description = $"The SSL Certificate is going to expire on: {site.SSLExpirationDate}. Additional info: _________________"
                };
                return View(model);
            }
            catch (Exception ex) 
            {
                //WorkOrderDTO model = new WorkOrderDTO()
                //{
                //    SiteID = siteId,
                //    SiteName = "___",
                //    SiteURL = "about:blank",
                //    IssueType = "Other",
                //    Category = "Help Desk",
                //    System = "Help Desk",
                //    ShortDescription = $"Update certificate for _____",
                //    Urgency = "2",
                //    Description = ""
                //};
                //return View(model);
                return BadRequest(ex.Message);
            }
            
        }





    }
}

