using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Models.DTO;
using ISHealthMonitor.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISHealthMonitor.Core.Contracts;
using System.Net.Http;
using System.Security.Policy;
using System.Text;

namespace ISHealthMonitor.UI.Controllers.Rest
{
    [Route("rest/api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SyncController : ControllerBase
    {
        
        private readonly ILogger<SyncController> _logger;
        private readonly IHealthModel _healthModel;
        public SyncController(ILogger<SyncController> logger, IHealthModel healthModel)
        {
            _logger = logger;
            _healthModel = healthModel;
        }

        [HttpGet(Name = "SyncSites")]
        public async Task<IActionResult> Get()
        {
			var failedSiteUrls = await _healthModel.UpdateSiteCerts();
			var fireEmailRemindersCount = await _healthModel.FireEmailReminders();

			if (failedSiteUrls.Count > 0)
			{
				return Ok(new { Message = "Failed", FailedSiteUrls = failedSiteUrls });
			}
			else
			{
				return Ok(new { Message = "Success. " + fireEmailRemindersCount + " were sent out." });
			}

		}

	}
}
