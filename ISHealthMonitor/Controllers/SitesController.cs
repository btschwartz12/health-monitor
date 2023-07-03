using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

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
    }
}
