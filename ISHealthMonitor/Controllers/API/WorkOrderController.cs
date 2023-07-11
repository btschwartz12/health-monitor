using ISHealthMonitor.Core.Contracts;

using ISHealthMonitor.Core.Helpers.WorkOrder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ISHealthMonitor.UI.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkOrderController: ControllerBase
    {
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;

        private readonly ILogger<WorkOrderController> _logger;
        public WorkOrderController(ILogger<WorkOrderController> logger,
            IHealthModel healthModel,
            IEmployee employee)
        {
            _logger = logger;
            _healthModel = healthModel;
            _employee = employee;
        }


        [HttpPost]
        [Route("CreateWorkOrder")]
        public IActionResult CreateWorkOrder([FromBody] WorkOrderModel model)
        {

            return Ok(model);
        }
    }
}
