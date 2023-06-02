using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace ISHealthMonitor.UI.Controllers
{
    public class SitesController : Controller
    {

        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;


        public SitesController(IHealthModel healthModel, IEmployee employee)
        {
            _healthModel = healthModel;
            _employee = employee;
        }

        public IActionResult Index()
        {
            var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var employee = _employee.GetEmployeeByUserName(username);

            ViewBag.UserIsAdmin = _healthModel.UserIsAdmin(new Guid(employee.GUID));
            ViewBag.UserName = username;

            return View();
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
                return View(newSiteDTO);
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
                };



                return View(siteDTO);
            }
        }
    }
}
