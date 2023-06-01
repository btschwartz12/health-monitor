using ISHealthMonitor.Core.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISHealthMonitor.Controllers.Data
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        private readonly IHealthModel _healthModel;
        private readonly ILogger<SiteController> _logger;
        public SiteController(ILogger<SiteController> logger,
            IHealthModel healthModel)
        {
            _logger = logger;
            _healthModel = healthModel;
          
        }
        [HttpGet("GetSites/{siteid}")]
        public string GetById(int siteid)
        {
            var retList = _healthModel.GetSites().Where(x=>x.id==siteid);
            return JsonConvert.SerializeObject(retList);
        }
        [HttpGet("GetSites")]
        public string GetSites()
        {
            var retList = _healthModel.GetSites();
            return JsonConvert.SerializeObject(retList);
        }
    }
}
