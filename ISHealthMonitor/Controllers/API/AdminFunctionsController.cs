using ISHealthMonitor.Core.Common;
using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.DataAccess;
using ISHealthMonitor.Core.Helpers.Confluence;
using ISHealthMonitor.Core.Helpers.Email;
using ISHealthMonitor.Core.Models;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISHealthMonitor.UI.Controllers.API
{

	[Route("api/[Controller]")]
    [ApiController]
    public class AdminFunctionsController : ControllerBase
    {
        private readonly IHealthModel _healthModel;


        public AdminFunctionsController(IHealthModel healthModel)
        {
            _healthModel = healthModel;
		}

		[HttpGet]
		[Route("updateconfluence")]
		public async Task<IActionResult> UpdateConfluence()
		{

			try
			{
				var (Message, responseData) = await _healthModel.UpdateConfluencePage();

				if (Message == "Success")
				{
					return Ok(new { Message = Message });
				}
				else
				{
					return Ok(new { Message = Message, responseData = responseData });
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}

		}



		[HttpGet]
		[Route("refreshcerts")]
		public async Task<IActionResult> RefreshCerts()
		{

			try
			{
				var (Message, FailedSiteUrls) = await _healthModel.UpdateCerts();

				if (Message == "Success")
				{
					return Ok(new { Message = Message });
				}
				else
				{
					return Ok(new { Message = Message, FailedSiteUrls = FailedSiteUrls });
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
			
		}



		[HttpGet]
		[Route("firereminders")]
		public async Task<IActionResult> FireReminders()
		{

			try
			{
				var (Message, remindersSent) = await _healthModel.FireReminders();

				if (Message == "Success")
				{
					return Ok(new { Message = Message, remindersSent = remindersSent });
				}
				else
				{
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}


	}




}

