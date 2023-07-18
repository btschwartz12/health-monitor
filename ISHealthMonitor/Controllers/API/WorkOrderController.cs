using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Helpers.WorkOrder;
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

            var unityModel = new UnityRestAPIAccess(_logger, _config);

            int objectid = -1;

            try
            {
                //objectid = await unityModel.GetRequestorId(employee.Email);
                objectid = await unityModel.GetRequestorId("Nick.Susanjar@hyland.com");

                if (objectid == -1)
                {
                    throw new Exception("Response does not contain a valid objectid");
                }
            }
            catch (Exception ex) 
            { 
                _logger.LogError("Failed to get requestor id: " + ex.Message);
                return BadRequest(ex.Message);
            }

            var workOrderModel = new WorkOrderModel(_logger);

            int linkToWorkOrderCategory = 36942305;
            int linkToSystemProfile = 34237289;
            string origin = "IS Health Monitor Site";
            string appName = "HSI CM";
            string className = "ISWorkOrder";
            int workOrderObjId = 0;

            var attrList = workOrderModel.CreateAttrList(objectid, linkToWorkOrderCategory,
                                                         linkToSystemProfile, model.ShortDescription,
                                                         model.Description, model.Urgency,
                                                         model.EmergencyReason, origin);





            OnbaseWorkviewObjectDTO wvObject = workOrderModel.GetWorkViewObjectDTO(workOrderObjId, appName,
                                                                                    className, attrList);

            var unityApi = new UnityRestAPIAccess(_logger, _config);

            try
            {
                string wvObjectJson = JsonSerializer.Serialize(wvObject);

                var objectId = await unityApi.CreateWorkViewObject(wvObject.appName, wvObject.className,
                                                        wvObjectJson);

                _healthModel.UpdateWorkOrderForSite(model.SiteID, int.Parse(objectId));

                _logger.LogInformation($"Work Order created by {employee.GUID} for siteID={model.SiteID}. Work Order object ID = {objectId}");

                return Ok(objectId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create work order: " + ex.Message);
                return BadRequest(ex.Message);
            }
            
        }
    }
}
