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

namespace ISHealthMonitor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHealthModel _healthModel;
        public HomeController(ILogger<HomeController> logger,
            IHealthModel healthModel)
        {
            _logger = logger;
            _healthModel = healthModel;
        }

        public IActionResult Index()
        {
            var user = HttpContext.User.Identity.Name;
                   
            return View();
        }
		
		public IActionResult AddEditSSLSite(int id=0)
        {
            var certDTO = new CertificateDTO();
            if (id != 0)
            {
                certDTO = _healthModel.GetSites().Where(x => x.id == id).First();
            }

			return View(certDTO);
		}
        [HttpPost]
        public IActionResult AddEditSSLSite(CertificateDTO ct)
        {
            var certDTO = new CertificateDTO()
            {
                SiteName = ct.SiteName,
                SiteURL = ct.SiteURL
            };
            //Save it
            return View(certDTO);
        }
        public IActionResult AddEditAzureSite(int id = 0)
        {
            var certDTO = new CertificateDTO();
            if (id != 0)
            {
                certDTO = _healthModel.GetSites().Where(x => x.id == id).First();
            }

            return View(certDTO);
        }
        [HttpPost]
        public IActionResult AddEditAzureSite(CertificateDTO ct)
        {
            var certDTO = new CertificateDTO()
            {
                SiteName = ct.SiteName,
                SiteURL = ct.SiteURL
            };
            //Save it
            return View(certDTO);
        }
        public IActionResult Configuration()
        {
            return View();
        }

      
    }
}
