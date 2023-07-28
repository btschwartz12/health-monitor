using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.Contexts;
using ISHealthMonitor.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ISHealthMonitor.Core.Contracts;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Implementations
{
	public class SplunkModel: ISplunkModel
	{

		private readonly IConfiguration _config;
		private readonly ILogger<HealthModel> _logger;

		public SplunkModel(IEmployee employee, IRest rest, IConfiguration configuration, ILogger<HealthModel> logger)
		{
			_config = configuration;
			_logger = logger;
		}

        public List<string> GetOperations()
        {
			var res = new List<string>()
			{
				"Equal",
				"Less Than",
				"Greater Than"
			};

			return res;
        }

        public List<string> GetTables()
        {
            var res = new List<string>()
            {
                "Table1",
                "Table2",
                "Table3"
            };

            return res;
        }

        public string Test()
		{
			return "Bruh";
		}
	}
}
