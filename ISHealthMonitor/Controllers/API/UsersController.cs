using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ISHealthMonitor.UI.Controllers.API
{
	[Route("api/[controller]")]
	[ApiController]
    //[Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
	{
		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;
        private readonly IConfiguration _config;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IHealthModel healthModel, IEmployee employee, ILogger<UsersController> logger, IConfiguration config)
        {
            _employee = employee;
            _healthModel = healthModel;
            _logger = logger;
            _config = config;
        }

        [HttpGet("GetUsers")]
		public string GetUsers()
		{
			var retList = _healthModel.GetUsers();
			return JsonConvert.SerializeObject(retList);
		}

        [HttpGet("GetLoggedInUser")]
        public IActionResult GetLoggedInUser()
        {
            var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			Dictionary<string, string> res = new Dictionary<string, string>() { };
			res.Add("username", username);

			var CurrentEmployee = _employee.GetEmployeeByUserName(username);

			var userIsAdmin = _healthModel.UserIsAdmin(new Guid(CurrentEmployee.GUID));

			if (userIsAdmin)
			{
				string apiUsername = _config.GetSection("ApiAuthConfig")["userName"];
				string apiPassword = _config.GetSection("ApiAuthConfig")["password"];

                res.Add("apiAuthUsername", apiUsername);
                res.Add("apiAuthPassword", apiPassword);
                res.Add("isAdmin", "true");

            }
            else
            {
                res.Add("isAdmin", "false");
            }

            return Ok(res);
        }

		[HttpPut]
		[Route("DeleteUser")]
		public IActionResult DeleteUser(int id)
		{

			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var employee = _employee.GetEmployeeByUserName(username);

			_healthModel.DeleteUser(id);
            _logger.LogInformation($"User ID={id.ToString()} deleted by {employee.GUID}");
            return Ok(id);
		}

		[HttpGet]
		[Route("GetAvailableUsers")]
		public IActionResult GetAvailableUsers()
		{
			List<EmployeeDTO> employees = _employee.GetAll();

			return Ok(employees);
		}

        [HttpPost]
        [Route("CreateUser")]
        public IActionResult CreateUser([FromBody] List<UserDTO> userDTOs)
        {
            var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

            var employee = _employee.GetEmployeeByUserName(username);


            if (userDTOs.All(u => u.ID == 0)) // When configuring multiple users at once
            {
                List<string> existingUsers = new List<string>();

                // Checking for existing users
                foreach (var userDTO in userDTOs)
                {
                    if (_healthModel.UserExists(new Guid(userDTO.Guid)))
                    {
                        existingUsers.Add(userDTO.DisplayName);
                    }
                }

                // If any existing users are found, return their names in a BadRequest
                if (existingUsers.Any())
                {
                    return BadRequest(new { Message = "User(s) already exist: " + string.Join(", ", existingUsers) });
                }

                // Creating new users
                foreach (var userDTO in userDTOs)
                {
                    var newUser = new ISHealthMonitorUserDbSet()
                    {
                        Guid = new Guid(userDTO.Guid),
                        IsAdmin = userDTO.IsAdmin,
                        DisplayName = userDTO.DisplayName,
                        Disabled = false,
                        Deleted = false,
                        DateCreated = DateTime.Now,
                    };

                    _logger.LogInformation($"User ({userDTO.Guid.ToString()}={userDTO.DisplayName}) created by {employee.GUID} with IsAdmin={userDTO.IsAdmin.ToString()}");

                    _healthModel.AddUser(newUser);
                }

                return Ok(new { Message = "Success" });
            }
            else if (userDTOs.Count == 1 && userDTOs[0].ID != 0) // When editing an existing user
            {
                var existingUser = _healthModel.GetUser(userDTOs[0].ID);
                if (existingUser != null)
                {
                    existingUser.IsAdmin = userDTOs[0].IsAdmin;
                    
                    _logger.LogInformation($"User ({existingUser.Guid.ToString()}={existingUser.DisplayName}) updated to IsAdmin={existingUser.IsAdmin.ToString()} by {employee.GUID}");

                    _healthModel.UpdateUser(existingUser);
                    return Ok(existingUser);
                }
                else
                {
                    return NotFound();
                }
            }
            else // When neither of the above conditions are met
            {
                return BadRequest(new { Message = "Invalid request" });
            }
        }

    }
}
