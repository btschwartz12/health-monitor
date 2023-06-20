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

namespace ISHealthMonitor.Controllers
{
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;
        private readonly IRest _restModel;
        public HomeController(ILogger<HomeController> logger,
            IHealthModel healthModel, IEmployee employee, IRest restModel)
        {
            _logger = logger;
            _employee = employee;
            _healthModel = healthModel;
            _restModel = restModel;
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

        public async Task<IActionResult> Configuration()
        {
            //example to get Page Source
            var action = "/wiki/api/v2/pages/300580865?body-format=storage";
            var rek = await _restModel.GetHttpContent(action);

            var confluencePage = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfluencePageInfo>(rek);


            //example to Update Page
            var pp = new ConfluenceAPIPage()
            {
                id = 300580865,
                spaceId = 300482563,
                title = "IS Health Monitor Resource",
                status = "current",
                version = new ISHealthMonitor.Core.DataAccess.Version()
                {
                    number = 7
                },
                body = new ISHealthMonitor.Core.DataAccess.Body()
                {
                    representation = "storage",
                    value = "<table data-layout=\"default\" ac:local-id=\"10fea502-e105-47a1-8476-486ae919b919\"><colgroup><col style=\"width: 136.0px;\" /><col style=\"width: 136.0px;\" /><col style=\"width: 136.0px;\" /><col style=\"width: 136.0px;\" /><col style=\"width: 136.0px;\" /></colgroup><tbody><tr><th><p><strong>Site Name</strong></p></th><th><p><strong>URL</strong></p></th><th><p><strong>SSL Name</strong></p></th><th><p><strong>Thumbprint</strong></p></th><th><p><strong>Expiration Date</strong></p></th></tr><tr><td>Pages Prod</td><td>https://pages.hyland.com/joseram</td><td>Entrust Certification Authority - L1K Joe Burrow</td><td>AB 46 0F 04 80 E3 7F 92 53 4E 2E F9 34 6C FA ED B9 23 B1 51</td><td>2/19/2024</td></tr><tr><td>Pages Dev</td><td>https://dev.pages.hyland.com</td><td>Entrust Certification Authority - L1K</td><td>9E 52 22 5B 5B 29 3F E0 E8 5F B7 68 D3 92 FE F9 27 16 76 88</td><td>2/19/2024</td></tr></tbody></table>"
                },
            };

            var rekt = await _restModel.PutHttpContent("/wiki/api/v2/pages/300580865", Newtonsoft.Json.JsonConvert.SerializeObject(pp), HttpContentTypes.String);




            return View();
        }







    }
}
