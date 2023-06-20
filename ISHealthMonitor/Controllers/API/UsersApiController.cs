using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
   // [AllowAnonymous]
    public class UsersApiController : ControllerBase
	{
		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;

        public UsersApiController(IHealthModel healthModel, IEmployee employee)
        {
            _employee = employee;
			_healthModel = healthModel;
        }

		[HttpGet("GetUsers")]
		public string GetUsers()
		{
			var retList = _healthModel.GetUsers();
			return JsonConvert.SerializeObject(retList);
		}

		[HttpPut]
		[Route("DeleteUser")]
		public IActionResult DeleteUser(int id)
		{
			_healthModel.DeleteUser(id);
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
