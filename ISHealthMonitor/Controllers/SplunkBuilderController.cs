using ISHealthMonitor.Core.Contracts;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISHealthMonitor.UI.Controllers
{
	//[Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
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
