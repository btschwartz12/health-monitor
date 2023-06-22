using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.Contexts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Helpers.Email;
using ISHealthMonitor.Core.Implementations;
using ISHealthMonitor.Core.Models;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ISHealthMonitor.UI.Controllers.API
{

    [Route("api/[controller]")]
	[ApiController]
    public class AdminFunctionsController : ControllerBase
    {
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;


		public AdminFunctionsController(IHealthModel healthModel, IEmployee employee)
        {
            _employee = employee;
            _healthModel = healthModel;
		}


		[HttpGet]
		[Route("refreshcerts")]
		public async Task<IActionResult> RefreshCerts()
		{
			var failedSiteUrls = await _healthModel.UpdateSiteCerts();

			if (failedSiteUrls.Count > 0)
			{
				return Ok(new { Message = "Failed", FailedSiteUrls = failedSiteUrls });
			}
			else
			{
				return Ok(new { Message = "Success" });
			}
		}



		[HttpGet]
		[Route("firereminders")]
		public async Task<IActionResult> FireReminders()
		{
			var fireEmailRemindersCount = await _healthModel.FireEmailReminders();

            return Ok(fireEmailRemindersCount); 
		}
	}

}

