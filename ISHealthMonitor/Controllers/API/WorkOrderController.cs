using ISHealthMonitor.Core.Contracts;

using ISHealthMonitor.Core.Helpers.WorkOrder;
using ISHealthMonitor.Core.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Text.Json;

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


        [HttpPost]
        [Route("CreateWorkOrder")]
        public IActionResult CreateWorkOrder([FromBody] WorkOrderDTO model)
        {
            var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");
            var employee = _employee.GetEmployeeByUserName(username);

            var unityModel = new UnityRestAPIAccess(_logger, _config);

            int objectid = -1;

            try
            {
                //objectid = unityModel.GetRequestorId(employee.Email);
                objectid = unityModel.GetRequestorId("Nick.Susanjar@hyland.com");

                if (objectid == -1)
                {
                    throw new Exception("Response does not contain a valid objectid");
                }
            }
            catch (Exception ex) 
            { 
                _logger.LogError(ex.Message);
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
                var retVal = unityApi.CreateWorkViewObject(wvObject.appName, wvObject.className,
                                                       JsonSerializer.Serialize(wvObject));

                return Ok(retVal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
