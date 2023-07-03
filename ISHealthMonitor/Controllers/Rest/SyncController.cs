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
using System.Text.Json;

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

        [HttpGet]
        [Route("Test")]
        public async Task<IActionResult> Test()
		{
			return Ok("Hello Nick");
		}

        [HttpGet]
		[Route("Start")]
		public async Task<IActionResult> Get()
		{
			var responseModel = new ApiResponseModel
			{
				TaskResults = new List<TaskResult>()
			};

			try
			{
				// First update the certs in the DB
				(string Message, Dictionary<string, string> FailedSiteUrls) = await _healthModel.UpdateCerts();

				var updateCertsResult = new UpdateCertsResult
				{
					FailedSiteUrls = FailedSiteUrls
				};

				responseModel.TaskResults.Add(new TaskResult
				{
					TaskName = "UpdateCerts",
					Status = Message,
					ErrorMessage = Message == "Success" ? null : "UpdateCerts failed",
					Data = updateCertsResult
				});

				_logger.LogInformation("UpdateCerts completed with status: {status}", Message);
			}
			catch (Exception ex)
			{
				responseModel.TaskResults.Add(new TaskResult
				{
					TaskName = "UpdateCerts",
					Status = "Failed",
					ErrorMessage = ex.Message
				});

				_logger.LogError("UpdateCerts threw an exception: {exception}", ex);
			}

			try
			{
				// Now update the confluence page
				(string Message, Dictionary<string, string> responseData) = await _healthModel.UpdateConfluencePage();

				var updateConfluencePageResult = new UpdateConfluencePageResult
				{
					ResponseData = responseData
				};

				responseModel.TaskResults.Add(new TaskResult
				{
					TaskName = "UpdateConfluencePage",
					Status = Message,
					ErrorMessage = Message == "Success" ? null : "UpdateConfluencePage failed",
					Data = updateConfluencePageResult
				});

				_logger.LogInformation("UpdateConfluencePage completed with status: {status}", Message);
			}
			catch (Exception ex)
			{
				responseModel.TaskResults.Add(new TaskResult
				{
					TaskName = "UpdateConfluencePage",
					Status = "Failed",
					ErrorMessage = ex.Message
				});

				_logger.LogError("UpdateConfluencePage threw an exception: {exception}", ex);
			}

			try
			{
				// Now send the email reminders
				(string Message, Dictionary<string, Dictionary<string, List<string>>> remindersSent) = await _healthModel.FireReminders();

				var fireRemindersResult = new FireRemindersResult
				{
					RemindersSent = remindersSent
				};

				responseModel.TaskResults.Add(new TaskResult
				{
					TaskName = "FireReminders",
					Status = Message,
					ErrorMessage = Message == "Success" ? null : "FireReminders failed",
					Data = fireRemindersResult
				});

				_logger.LogInformation("FireReminders completed with status: {status}", Message);
			}
			catch (Exception ex)
			{
				responseModel.TaskResults.Add(new TaskResult
				{
					TaskName = "FireReminders",
					Status = "Failed",
					ErrorMessage = ex.Message
				});

				_logger.LogError("FireReminders threw an exception: {exception}", ex);
			}

			if (responseModel.TaskResults.All(r => r.Status == "Success"))
			{
				responseModel.Status = "Success";
				responseModel.Message = "All tasks completed successfully";
			}
			else
			{
				responseModel.Status = "Failed";
				responseModel.Message = "One or more tasks failed. Check the TaskResults for more information.";
			}


            string jsonString = JsonSerializer.Serialize(responseModel, new JsonSerializerOptions { WriteIndented = true });

			_logger.LogInformation($"Workflow completed. Results:");
			_logger.LogInformation($"{jsonString}");

            return Ok(responseModel);
		}


	}

	public class ApiResponseModel
	{
		public string Status { get; set; }
		public string Message { get; set; }
		public List<TaskResult> TaskResults { get; set; }
	}

	public class TaskResult
	{
		public string TaskName { get; set; }
		public string Status { get; set; }
		public string ErrorMessage { get; set; }
		public object Data { get; set; }
	}

	public class UpdateCertsResult
	{
		public Dictionary<string, string> FailedSiteUrls { get; set; }
	}

	public class UpdateConfluencePageResult
	{
		public Dictionary<string, string> ResponseData { get; set; }
	}

	public class FireRemindersResult
	{
		public Dictionary<string, Dictionary<string, List<string>>> RemindersSent { get; set; }
	}

}
