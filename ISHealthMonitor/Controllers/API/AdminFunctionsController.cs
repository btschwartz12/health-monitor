using ISHealthMonitor.Core.Common;
using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.DataAccess;
using ISHealthMonitor.Core.Helpers.Confluence;
using ISHealthMonitor.Core.Helpers.Email;
using ISHealthMonitor.Core.Implementations;
using ISHealthMonitor.Core.Models;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISHealthMonitor.UI.Controllers.API
{

	[Route("api/[Controller]")]
    [ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	//[Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
	//[Authorize(Policy = "Admin")]
	public class AdminFunctionsController : ControllerBase
    {
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;
        private readonly ILogger<AdminFunctionsController> _logger;


        public AdminFunctionsController(IHealthModel healthModel, IEmployee employee, ILogger<AdminFunctionsController> logger)
        {
            _healthModel = healthModel;
			_logger = logger;
			_employee = employee;
		}

		[HttpGet]
		[Route("updateconfluence")]
		public async Task<IActionResult> UpdateConfluence(string username)
		{

            var employee = _employee.GetEmployeeByUserName(username);

            try
			{
				var (Message, responseData) = await _healthModel.UpdateConfluencePage();

				if (Message == "Success")
				{
					_logger.LogInformation($"Confluence page successfully updated by {employee.GUID}");
					return Ok(new { Message = Message });
				}
				else
				{
					_logger.LogInformation($"Confluence page failed to update (initiated by {employee.GUID}): {responseData.ToString()}");
					return Ok(new { Message = Message, responseData = responseData });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in Confluence Update endpoint (initiated by {employee.GUID}): {ex}");
				return BadRequest(ex.Message);
			}

		}



		[HttpGet]
		[Route("refreshcerts")]
		public async Task<IActionResult> RefreshCerts(string username)
		{
            var employee = _employee.GetEmployeeByUserName(username);

            try
			{
				var (Message, FailedSiteUrls) = await _healthModel.UpdateCerts();
                if (Message == "Success")
				{
                    _logger.LogInformation($"Site Certificate data successfully updated in DB by {employee.GUID}");
                    return Ok(new { Message = Message });
				}
				else
				{
                    _logger.LogInformation($"Site Certificate data partially successfully updated in DB by {employee.GUID} (failedSites = {FailedSiteUrls})");
                    return Ok(new { Message = Message, FailedSiteUrls = FailedSiteUrls });
				}
			}
			catch (Exception ex)
			{
                _logger.LogError($"Error in Refresh Certs endpoint (initiated by {employee.GUID}): {ex}");
                return BadRequest(ex.Message);
			}
			
		}



		[HttpGet]
		[Route("firereminders")]
		public async Task<IActionResult> FireReminders(string username)
		{

            var employee = _employee.GetEmployeeByUserName(username);

            try
			{
				var (Message, remindersSent) = await _healthModel.FireReminders();

				if (Message == "Success")
				{
                    _logger.LogInformation($"Reminder emails successfully sent out (initiated by {employee.GUID})");

					string jsonString = JsonSerializer.Serialize(remindersSent, new JsonSerializerOptions { WriteIndented = true });

					_logger.LogInformation($"Reminder Data: {jsonString}");
                    return Ok(new { Message = Message, remindersSent = remindersSent });
				}
				else
				{
                    _logger.LogInformation($"Reminder emails failed to fire (initiated by {employee.GUID})");
                    return BadRequest();
				}
			}
			catch (Exception ex)
			{
                _logger.LogError($"Error in Fire Reminders endpoint (initiated by {employee.GUID}): {ex}");
                return BadRequest(ex.Message);
			}
		}

        [HttpGet]
        [Route("sendautoworkorders")]
        public async Task<IActionResult> SendAutoWorkOrders(string username)
        {

            var employee = _employee.GetEmployeeByUserName(username);

            try
            {
				var (Message, workOrdersCreated, sitesWithExistingWorkOrders) = await _healthModel.AutoCreateWorkOrders(employee);

                if (Message == "Success")
                {
                    _logger.LogInformation($"Automatic work orders successfully created (initiated by {employee.GUID})");

                    string WOCreatedjsonString = JsonSerializer.Serialize(workOrdersCreated, new JsonSerializerOptions { WriteIndented = true });
					string ExistingWOjsonString = JsonSerializer.Serialize(sitesWithExistingWorkOrders, new JsonSerializerOptions { WriteIndented = true });

                    _logger.LogInformation($"Created Work Order Data: {WOCreatedjsonString}");
                    _logger.LogInformation($"Existing Work Order Data: {ExistingWOjsonString}");
                    return Ok(new { Message = Message, WorkOrdersCreated = workOrdersCreated, SitesWithExistingWorkOrders = sitesWithExistingWorkOrders });
                }
                else
                {
                    _logger.LogInformation($"Automatic work orders failed to fire (initiated by {employee.GUID}). Message: " + Message);
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in automatic work orders endpoint (initiated by {employee.GUID}): {ex}");
                return BadRequest(ex.Message);
            }
        }


    }




}

