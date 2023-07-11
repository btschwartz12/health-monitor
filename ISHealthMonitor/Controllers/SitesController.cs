using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Data.Models;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace ISHealthMonitor.UI.Controllers
{
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public class SitesController : Controller
    {

        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;
        private readonly IConfiguration _config;


        public SitesController(IHealthModel healthModel, IEmployee employee, IConfiguration config)
        {
            _healthModel = healthModel;
            _employee = employee;
            _config = config;
        }

        public IActionResult Index()
        {
            var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var employee = _employee.GetEmployeeByUserName(user);

            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(employee.GUID));
            ViewBag.UserName = employee.DisplayName;


            if (ViewBag.UserIsAdmin)
            {
                string username = _config.GetSection("ApiAuthConfig")["userName"];
                string password = _config.GetSection("ApiAuthConfig")["password"];

                ViewBag.ApiAuthUserName = username;
                ViewBag.ApiAuthPassword = password;
            }

            return View("~/Views/Home/Sites.cshtml");
        }

        [HttpGet]
        public IActionResult AddEdit(int id = 0)
        {
            if (id == 0)
            {
                SiteDTO newSiteDTO = new()
                {
                    SiteName = "New Site",

                };
                return View("~/Views/Admin/Sites/AddEdit.cshtml", newSiteDTO);
            }
            else
            {
                ISHealthMonitorSiteDbSet site = _healthModel.GetSite(id);

                SiteDTO siteDTO = new()
                {
                    ID = site.ID,
                    SiteName = site.DisplayName,
                    SiteURL = site.URL,
                    SiteCategory = site.SiteCategory,
                    SSLEffectiveDate = site.SSLEffectiveDate.ToString("yyyy-MM-dd"),
                    SSLExpirationDate = site.SSLExpirationDate.ToString("yyyy-MM-dd"),
                    SSLIssuer = site.SSLIssuer,
                    SSLSubject = site.SSLSubject,
                    SSLCommonName = site.SSLCommonName,
                    SSLThumbprint = site.SSLThumbprint
                };



                return View("~/Views/Admin/Sites/AddEdit.cshtml", siteDTO);
            }
        }

        public IActionResult SiteStatusViewer()
        {
			var user = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var employee = _employee.GetEmployeeByUserName(user);

			ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(employee.GUID));
			ViewBag.UserName = employee.DisplayName;


			if (ViewBag.UserIsAdmin)
			{
				string username = _config.GetSection("ApiAuthConfig")["userName"];
				string password = _config.GetSection("ApiAuthConfig")["password"];

				ViewBag.ApiAuthUserName = username;
				ViewBag.ApiAuthPassword = password;
			}


            return View("~/Views/Home/SiteStatusViewer.cshtml");
		}


        public IActionResult SiteSubscriptions(int siteId)
        {
            SiteDTO site = _healthModel.GetSiteDTO(siteId);

            ViewBag.SiteName = site.SiteName;
            ViewBag.SiteURL = site.SiteURL;

            try
            {
                Dictionary<string, List<string>> usersSubscribed = _healthModel.GetSubscriptionsForSite(siteId);

                // Removing duplicates and adding count.
                var usersSubscribedProcessed = new Dictionary<string, List<string>>();
                foreach (var user in usersSubscribed)
                {
                    var reminders = new List<string>();
                    var reminderCounts = user.Value.GroupBy(r => r)
                                                .ToDictionary(g => g.Key, g => g.Count());

                    foreach (var reminder in reminderCounts)
                    {
                        reminders.Add(reminder.Value > 1 ? $"{reminder.Key} x {reminder.Value}" : reminder.Key);
                    }

                    usersSubscribedProcessed.Add(user.Key, reminders);
                }

                ViewBag.UsersSubscribed = usersSubscribedProcessed;

                return View("~/Views/Home/SiteSubscriptions.cshtml");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
