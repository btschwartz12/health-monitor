using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISHealthMonitor.UI.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkOrderController: ControllerBase
    {
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;
        private readonly IConfiguration _config;


        private readonly ILogger<WorkOrderController> _logger;
        public WorkOrderController(ILogger<WorkOrderController> logger,
            IHealthModel healthModel,
            IEmployee employee,
            IConfiguration config)
        {
            _logger = logger;
            _healthModel = healthModel;
            _employee = employee;
            _config = config;
        }


        [HttpGet]
        [Route("CheckWorkOrderStatus")]
        public async Task<IActionResult> CheckWorkOrderStatus(int siteId)
        {
            ISHealthMonitorSiteDbSet site = _healthModel.GetSite(siteId);

            if (site == null)
            {
                return NotFound();
            }

            if (site.HSIDBWorkOrderCurrentObjectID == null || site.HSIDBWorkOrderLastSubmittedDate == null)
            {
                return Ok(new { Message = "Ok" });
            }

            if (site.HSIDBWorkOrderLastSubmittedDate <= site.SSLEffectiveDate)
            {
                return Ok(new { Message = "Ok" });
            }

            var workOrderBaseUrl = _config.GetSection("UnityRestAPI")["HSIDBWorkOrderURL"];

            var objId = site.HSIDBWorkOrderCurrentObjectID;
            var workOrderViewUrl = workOrderBaseUrl + objId;
            var submittedDate = site.HSIDBWorkOrderLastSubmittedDate.ToString();
            var certEffectiveDate = site.SSLEffectiveDate.ToString();

            var warningData = new Dictionary<string, string>
            {
                { "workOrderObjectId", objId.ToString() },
                { "workOrderViewUrl", workOrderViewUrl },
                { "workOrderSubmissionDate", submittedDate },
                { "siteCertEffectiveDate", certEffectiveDate },
            };

            return Ok(new { Message = "Warning", WarningData = warningData });
        }


        [HttpPost]
        [Route("CreateWorkOrder")]
        public async Task<IActionResult> CreateWorkOrder([FromBody] WorkOrderDTO model)
        {
            var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");
            var employee = _employee.GetEmployeeByUserName(username);


            Dictionary<string, string> resp = await _healthModel.CreateWorkOrder(model, employee);


            if (resp["Message"] == "Success")
            {
                var objectId = int.Parse(resp["ObjectID"]);
                return Ok(objectId);
            }
            if (resp["Message"] == "Failed")
            {
                return BadRequest(resp["Description"]);
            }

            return BadRequest();
        }
    }
}
