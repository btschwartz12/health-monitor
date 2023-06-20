using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ISHealthMonitor.UI.Controllers.Rest
{
    [Route("rest/api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SyncController : ControllerBase
    {
        
        private readonly ILogger<SyncController> _logger;

        public SyncController(ILogger<SyncController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "SyncSites")]
        public string Get()
        {
            return "AngelOfDeath";
        }
    }
}
