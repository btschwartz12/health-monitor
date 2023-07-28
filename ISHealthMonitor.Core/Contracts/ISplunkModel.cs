using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Contracts
{
	public interface ISplunkModel
	{
		string Test();


		List<string> GetTables();

		List<string> GetOperations();
	}
}
