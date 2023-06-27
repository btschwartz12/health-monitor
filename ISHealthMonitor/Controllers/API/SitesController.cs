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

namespace ISHealthMonitor.UI.Controllers.API
{
	[Route("api/[controller]")]
	[ApiController]
  
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

			_logger.LogInformation("Showing all sites to: " + employee.DisplayName);



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
            }

            return JsonConvert.SerializeObject(retList);
		}

		[HttpPut]
		[Route("DeleteSite")]
		public IActionResult DeleteSite(int id)
		{
			List<string> subscribedUsers = _healthModel.GetSubscribedUsersForSite(id);

			if (subscribedUsers.Count > 0)
			{
				return BadRequest(new { SubscribedUsers = subscribedUsers });
			}
			else
			{
				_healthModel.DeleteSite(id);
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
					Active = true,
					Deleted = false,
					Disabled = false
				};

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

					_healthModel.UpdateSite(existingSite);
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


		private async Task<SiteDTO> GetSiteDTO(string name, string category, string url, CertificateHandlers certHandlers)
		{
			try
			{
				CertificateDTO res = await certHandlers.CheckSSLSiteAsync(url);
				if (res.Issuer == null || res.Subject == null || res.EffectiveDate == null || res.ExpDate == null)
				{
					throw new Exception("null value in cert");
				}
				return new SiteDTO()
				{
					SiteURL = url,
					SiteName = name,
					SiteCategory = category,
					SSLEffectiveDate = res.EffectiveDate,
					SSLExpirationDate = res.ExpDate,
					SSLIssuer = res.Issuer,
					SSLSubject = res.Subject,
					SSLCommonName = res.CommonName,
					SSLThumbprint = res.Thumbprint
				};

			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}

        private async Task<List<SiteDTO>> ProcessJsonFileAsync(string filePath, string category)
		{
			// Prepare the result list
			List<SiteDTO> result = new List<SiteDTO>();

			// Initialize certificate handler
			var certHandlers = new CertificateHandlers();

			// Load json file
			string json = await System.IO.File.ReadAllTextAsync(filePath);

			// Parse json file
			var sites = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);


			List<string> failedUrls = new List<string>();

			// Iterate through each key-value pair
			foreach (var site in sites)
			{
				string name = site.Key;
				var urls = site.Value;




				// Check if urls list is empty
				if (urls.Count == 0) continue;

				for (int i = 0; i < urls.Count; i++)
				{
					string url = urls[i];
					string finalName = name;

					// Check if url contains 'dev' and is the second url
					if (url.Contains("dev") && urls.Count == 2)
					{
						finalName += " Dev";
					}
					// If there are more than two urls, append the URL to the end of the name
					else if (urls.Count > 2)
					{
						finalName += " " + url;
					}

					// Call GetSiteDTO function and add the result to the list
					try
					{
						SiteDTO siteDto = await GetSiteDTO(finalName, category, url, certHandlers);
						result.Add(siteDto);
					}
					catch (Exception ex)
					{
						failedUrls.Add(url);
						Console.WriteLine(ex);
					}
					
				}
			}

			return result;
		}


        private void CreateSiteInternal(SiteDTO siteDTO)
		{

			var username = HttpContext.User.Identity.Name.Replace("ONBASE\\", "");

			var employee = _employee.GetEmployeeByUserName(username);


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
				Active = true,
				Deleted = false,
				Disabled = false
			};

			_healthModel.AddSite(newSite);
		}













	}
}
