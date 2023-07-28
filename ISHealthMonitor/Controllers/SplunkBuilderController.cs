using ISHealthMonitor.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ISHealthMonitor.UI.Controllers
{
	public class SplunkBuilderController : Controller
	{

		private readonly ISplunkModel _splunk;

		public SplunkBuilderController(ISplunkModel splunk)
		{
			_splunk = splunk;
		}
		public IActionResult Index()
		{
			return View();
		}
	}
}
