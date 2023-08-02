using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.Models;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;

namespace ISHealthMonitor.UI.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
	[Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
	public class SplunkBuilderController : ControllerBase
    {
        private readonly ISplunkModel _splunk;

        public SplunkBuilderController(ISplunkModel splunk)
        {
            _splunk = splunk;
        }


        [HttpGet("GetTables")]
        public string GetTables()
        {
            var retList = _splunk.GetTables();
            return JsonConvert.SerializeObject(retList);
        }

        [HttpGet("GetOperations")]
        public string GetOperations()
        {
            var retList = _splunk.GetOperations();
            return JsonConvert.SerializeObject(retList);
        }


        [HttpPost("BuildQuery")]
        public string BuildQuery([FromBody] QueryStructure queryStructure)
        {
            var query = $"SELECT * FROM {queryStructure.Table}";

            if (queryStructure.Filters != null && queryStructure.Filters.Count > 0)
            {
                var filterQueries = queryStructure.Filters.Select(f => $"{f.Column} {f.Operation} '{f.Value}'");
                query += $" WHERE {string.Join(" AND ", filterQueries)}";
            }

            if (queryStructure.Sort != null)
            {
                query += $" ORDER BY {queryStructure.Sort.Column} {queryStructure.Sort.Order}";
            }

            return query;
        }
    }
}
