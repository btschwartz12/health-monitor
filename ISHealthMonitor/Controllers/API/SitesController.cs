using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Data.Models;
using ISHealthMonitor.Core.Models.DTO;
using ISHealthMonitor.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ISHealthMonitor.Core.Implementations;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Policy;

namespace ISHealthMonitor.UI.Controllers.API
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
	public class SitesController : ControllerBase
	{
		private readonly IHealthModel _healthModel;
		private readonly IEmployee _employee;

		private readonly ILogger<SitesController> _logger;
		public SitesController(ILogger<SitesController> logger,
			IHealthModel healthModel,
			IEmployee employee)
		{
			_logger = logger;
			_healthModel = healthModel;
			_employee = employee;
		}



		[HttpGet("GetSites")]
		public async Task<string> GetSitesAsync()
		{

            //List<SiteDTO> siteDtoList = await ProcessJsonFileAsync(@"C:\Users\bschwartz\Downloads\data.json", "IS Application");

            //foreach (SiteDTO site in siteDtoList)
            //{
            //	CreateSiteInternal(site);
            //}

            var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

            var employee = _employee.GetEmployeeByUserName(username);




			List<SiteDTO> retList = _healthModel.GetSites()
                .Where(site => site.SiteCategory != "All")
				.ToList();


            foreach (SiteDTO site in retList)
            {
                if (!string.IsNullOrEmpty(site.SSLThumbprint))
                {
                    var formatted = string.Join(" ", Enumerable.Range(0, site.SSLThumbprint.Length / 2).Select(i => site.SSLThumbprint.Substring(i * 2, 2)));
                    site.SSLThumbprint = formatted;
                }

				site.SiteName = $"<a target='_blank' href={site.SiteURL}>{site.SiteName}</a>";
            }

            return JsonConvert.SerializeObject(retList);
		}

		[HttpPut]
		[Route("DeleteSite")]
		public IActionResult DeleteSite(int id)
		{

            var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

            var employee = _employee.GetEmployeeByUserName(username);


            List<string> subscribedUsers = _healthModel.GetSubscribedUsersForSite(id);

			if (subscribedUsers.Count > 0)
			{
                _logger.LogInformation($"Site ID={id.ToString()} failed to delete (existing users subscribed)");
                return BadRequest(new { SubscribedUsers = subscribedUsers });
			}
			else
			{
                _healthModel.DeleteSite(id);
                _logger.LogInformation($"Site ID={id.ToString()} deleted by {employee.GUID}");
                return Ok(id);
			}
		}


		[HttpPost]
		[Route("CreateSite")]
		public IActionResult CreateSite([FromBody] SiteDTO siteDTO)
		{

			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var employee = _employee.GetEmployeeByUserName(username);


			if (siteDTO.ID == 0)
			{
				var newSite = new ISHealthMonitorSiteDbSet()
				{
					URL = siteDTO.SiteURL,
					DisplayName = siteDTO.SiteName,
					SiteCategory = siteDTO.SiteCategory,
					SSLEffectiveDate = DateTime.Parse(siteDTO.SSLEffectiveDate),
					SSLExpirationDate = DateTime.Parse(siteDTO.SSLExpirationDate),
					SSLIssuer = siteDTO.SSLIssuer,
					SSLSubject = siteDTO.SSLSubject,
					SSLCommonName = siteDTO.SSLCommonName,
					SSLThumbprint = siteDTO.SSLThumbprint,
					CreatedBy = new Guid(employee.GUID),
					DateCreated = DateTime.Now,
					LastUpdated = DateTime.Now,
					Active = true,
					Deleted = false,
					Disabled = false
				};

                _logger.LogInformation($"Site created by {employee.GUID}: ({siteDTO.SiteName} = {siteDTO.SiteURL})");

                _healthModel.AddSite(newSite);
				return Ok(newSite);
			}
			else
			{
				var existingSite = _healthModel.GetSite(siteDTO.ID);
				if (existingSite != null)
				{
					existingSite.URL = siteDTO.SiteURL;
					existingSite.DisplayName = siteDTO.SiteName;
					existingSite.SiteCategory = siteDTO.SiteCategory;
					existingSite.SSLEffectiveDate = DateTime.Parse(siteDTO.SSLEffectiveDate);
					existingSite.SSLExpirationDate = DateTime.Parse(siteDTO.SSLExpirationDate);
					existingSite.SSLIssuer = siteDTO.SSLIssuer;
					existingSite.SSLSubject = siteDTO.SSLSubject;
					existingSite.SSLCommonName = siteDTO.SSLCommonName;
					existingSite.SSLThumbprint = siteDTO.SSLThumbprint;
					existingSite.LastUpdated = DateTime.Now;

					_healthModel.UpdateSite(existingSite);

                    _logger.LogInformation($"Site ID={siteDTO.ID.ToString()} updated by {employee.GUID} to: ({siteDTO.SiteName} = {siteDTO.SiteURL}, and possibly other fields) ");

                    return Ok(existingSite);
				}
				else
				{
					return NotFound();
				}
			}
		}



        [HttpGet]
        [Route("GetSitesToSelect")]
        public async Task<IActionResult> GetSitesToSelect()
        {
            List<SiteDTO> retList = _healthModel.GetSites();

            var modifiedList = retList.Select(s => new
            {
                s.ID,
                s.SiteURL,
				s.SiteCategory,
                s.SiteName,
            }).ToList();


            return Ok(modifiedList);
        }


		[HttpGet]
		[Route("GetSiteReminderConfigurations")]
		public async Task<IActionResult> GetSiteReminderConfigurations()
		{
			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var employee = _employee.GetEmployeeByUserName(username);

			List<SiteReminderConfiguration> siteReminderConfigurations = _healthModel.GetSiteReminderConfigurations(new Guid(employee.GUID));

			return Ok(JsonConvert.SerializeObject(siteReminderConfigurations));


		}


		[HttpPost]
		[Route("GetSiteCertificate")]
		public async Task<IActionResult> GetSiteCertificate([FromBody] string url)
		{

			var certHandlers = new CertificateHandlers();

			try
			{
				CertificateDTO res = await certHandlers.CheckSSLSiteAsync(url);
				return Ok(JsonConvert.SerializeObject(res));
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}


		[HttpGet("GetSiteStatusData")]
		public async Task<IActionResult> GetSiteStatusData()
		{
			SiteStatusData model = new SiteStatusData()
			{
				SiteStatusList = new List<SiteStatus>() { }
			};

            List<SiteDTO> allSites = _healthModel.GetSites();

            List<SiteDTO> uniqueSiteDTOs = allSites
                .Where(s => s.ID != 1) // Filter out all sites
                .ToList();

            List<UserReminderDTO> allReminders = _healthModel.GetReminders();

            List<UserReminderDTO> remindersForAllSites = allReminders
                .Where(s => s.ISHealthMonitorSiteID == 1)
                .ToList();

            List<ReminderIntervalDTO> allReminderIntervals = _healthModel.GetReminderIntervals();
            Dictionary<int, (int, string)> reminderIntervalDictionary = allReminderIntervals.ToDictionary(x => x.ID, x => (x.DurationInMinutes, x.DisplayName));



            foreach (SiteDTO site in uniqueSiteDTOs)
			{
				SiteStatus siteStatus = new SiteStatus();

				DateTime expDate = DateTime.Parse(site.SSLExpirationDate);

				siteStatus.SiteID = site.ID;
				siteStatus.SiteURL = site.SiteURL;
				siteStatus.SiteName = $"<a target='_blank' href={site.SiteURL}>{site.SiteName}</a>";
				siteStatus.SSLExpirationDate = site.SSLExpirationDate;
				siteStatus.SSLCommonName = site.SSLCommonName;
				siteStatus.TimeUntilExpiration = _healthModel.GetTimeDiffString(expDate);
				siteStatus.RowColor = _healthModel.GetTimeDiffColor(expDate, site.SSLCommonName);
				siteStatus.Action = $"<div class='text-center'><i style='cursor: pointer;' class='fa fa-info fa-lg text-primary mr-3' " +
					$"onclick='showSiteSubscriptionsModal({site.ID})'></i></div>";
                siteStatus.WorkOrderAction = $"<div class='text-center'><a href='/Home/WorkOrderBuilder/?siteId={site.ID}'><i style='cursor: pointer;' class='fa fa-solid fa-wrench mr-3'></i></a></div>";



                siteStatus.NumSubscribedUsers = _healthModel.GetNumSubscriptionsForSite(site.ID);

				model.SiteStatusList.Add(siteStatus);

			}

			return Ok(JsonConvert.SerializeObject(model));
		}


    }
}
