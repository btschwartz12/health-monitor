using ISHealthMonitor.Core.Common;
using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.DataAccess;
using ISHealthMonitor.Core.Helpers.Confluence;
using ISHealthMonitor.Core.Helpers.Email;
using ISHealthMonitor.Core.Models;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISHealthMonitor.UI.Controllers.API
{

	[Route("api/boomi")]
    [ApiController]
    public class BoomiApiController : ControllerBase
    {
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;
        private readonly IRest _restModel;
		private readonly IConfiguration _configuration;


        public BoomiApiController(IHealthModel healthModel, IEmployee employee, IRest rest, IConfiguration configuration)
        {
            _employee = employee;
            _healthModel = healthModel;
			_restModel = rest;
			_configuration = configuration;
		}

		[HttpGet]
		[Route("updateconfluence")]
		public async Task<IActionResult> UpdateConfluence()
		{

			List<SiteDTO> sites = _healthModel.GetSites()
                .Where(site => site.SiteCategory != "All" && site.SiteCategory != "Test")
                .OrderBy(site => site.SiteName).ToList();

            ConfluenceTableModel model = new ConfluenceTableModel()
            {
                sites = sites
			};


			string rootDir = _configuration.GetSection("TemplatePaths")["ConfluenceTable"];


			var tableStr = ConfluenceTableHelper.GetSiteTableHTML(model, rootDir);


			//example to get Page Source
			var action = "/wiki/api/v2/pages/300580865?body-format=storage";
            var rek = await _restModel.GetHttpContent(action);

            var confluencePage = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfluencePageInfo>(rek);

			int curVersion = confluencePage.version.number;


			//example to Update Page
			var pp = new ConfluenceAPIPage()
			{
				id = 300580865,
				spaceId = 300482563,
				title = "IS Health Monitor Resource",
				status = "current",
				version = new ISHealthMonitor.Core.DataAccess.Version()
				{
					number = curVersion + 1
                },
                body = new ISHealthMonitor.Core.DataAccess.Body()
                {
                    representation = "storage",
                    value = tableStr
                },
            };

			try
			{
                var rekt = await _restModel.PutHttpContent("/wiki/api/v2/pages/300580865", Newtonsoft.Json.JsonConvert.SerializeObject(pp), HttpContentTypes.String);

				return Ok("Success");
            }
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}

        }



		[HttpGet]
		[Route("refreshcerts")]
		public async Task<IActionResult> RefreshCerts()
		{


            List<ISHealthMonitorSiteDbSet> sites = await _healthModel.GetSiteDbSets();

			sites.RemoveAll(site => site.ID == 1); // Disregard the row for 'All Sites'

			var certHandlers = new CertificateHandlers();

			var failedSiteUrls = new Dictionary<string, string>();

			foreach (var site in sites)
			{
				try
				{
					CertificateDTO cert = await certHandlers.CheckSSLSiteAsync(site.URL);

					site.SSLEffectiveDate = DateTime.Parse(cert.EffectiveDate);
					site.SSLExpirationDate = DateTime.Parse(cert.ExpDate);
					site.SSLIssuer = cert.Issuer;
					site.SSLSubject = cert.Subject;
					site.SSLCommonName = cert.CommonName;
					site.SSLThumbprint = cert.Thumbprint;

					if (cert.ErrorCommonName)
					{
						site.SSLCommonName = "INVALID (" + cert.CommonName + ")";
                        _healthModel.UpdateSite(site);
                        throw new Exception("Invalid SSL Common Name for the site.");
						
					}
					else
					{
                        _healthModel.UpdateSite(site);
                    }

					
				}
				catch (Exception ex)
				{
					failedSiteUrls.Add(site.URL, ex.Message);
				}

			}

			if (failedSiteUrls.Count > 0)
			{
				return Ok(new { Message = "Failed", FailedSiteUrls = failedSiteUrls });
			}
			else
			{
				return Ok(new { Message = "Success" });
			}
		}



		[HttpGet]
		[Route("firereminders")]
		public async Task<IActionResult> FireReminders()
		{


			string rootDir = _configuration.GetSection("TemplatePaths")["EmailReminder"];

			List<EmailReminderModel> emailModels = new List<EmailReminderModel>() { };

			var nearExpiredSites = await GetNearExpiredSites();
			var remindersList = await GetRemindersForNearExpiredSites(nearExpiredSites);
			List<RemindersToSendForSite> filteredReminderList = RemoveDuplicates(remindersList);

			foreach (var siteReminders in filteredReminderList)
			{
				var emailList = new List<string>() { };

				foreach (var reminder in siteReminders.Reminders)
				{
					if (reminder.CreatedBy.HasValue)
					{
						var email = _employee.GetEmailByGuid(reminder.CreatedBy.Value);

						if (!string.IsNullOrEmpty(email)) 
							emailList.Add(email);
					}
				}

				if (emailList.Count > 0)
				{
					var model = new EmailReminderModel
					{
						Emails = emailList,
						SiteURL = siteReminders.Site.SiteURL,
						SiteName = siteReminders.Site.SiteName,
						SSLEffectiveDate = siteReminders.Site.SSLEffectiveDate,
						SSLExpirationDate = siteReminders.Site.SSLExpirationDate,
						SSLIssuer = siteReminders.Site.SSLIssuer,
						SSLSubject = siteReminders.Site.SSLSubject,
						SSLCommonName = siteReminders.Site.SSLCommonName,
						SSLThumbprint = siteReminders.Site.SSLThumbprint,
                        IntervalDisplayName = siteReminders.ReminderInterval.DisplayName
                    };

                    emailModels.Add(model);
                }
                
            }

			int key = 0;

			foreach (var emailModel in emailModels)
			{
				EmailHelper.SendEmail(emailModel, rootDir, key);
				key += 1;
			}

			var result = new Dictionary<string, Dictionary<string, List<string>>>();

			foreach (var reminder in remindersList)
			{
				string siteUrl = reminder.Site.SiteURL;
				string intervalDisplayName = reminder.ReminderInterval.DisplayName;

				if (!result.ContainsKey(siteUrl))
				{
					result[siteUrl] = new Dictionary<string, List<string>>();
				}

				if (!result[siteUrl].ContainsKey(intervalDisplayName))
				{
					result[siteUrl][intervalDisplayName] = new List<string>();
				}

				foreach (var userReminder in reminder.Reminders)
				{
					result[siteUrl][intervalDisplayName].Add(userReminder.CreatedBy.ToString());
				}
			}

			return Ok(new { result });
		}


		private async Task<NearExpiredSites> GetNearExpiredSites()
		{
			var nearExpiredSites = new NearExpiredSites
			{
				SitesByIntervalDict = new Dictionary<ReminderIntervalDTO, List<SiteDTO>>()
			};

			var now = DateTime.Now;
			List<ISHealthMonitorSiteDbSet> sites = await _healthModel.GetSiteDbSets();
			List<ISHealthMonitorIntervalDbSet> intervals = await _healthModel.GetReminderIntervalDbSets();

			foreach (var interval in intervals)
			{
                var minutes = interval.DurationInMinutes;
                var timeSpan = TimeSpan.FromMinutes(minutes);
                var lowerBound = now.Date + timeSpan;
                var upperBound = lowerBound + TimeSpan.FromDays(1);

                // Get all sites that are within the interval (need a reminder sent)
                var sitesWithinInterval = sites.Where(site =>
                {
                    var expiryDate = site.SSLExpirationDate;
                    return expiryDate >= lowerBound && expiryDate < upperBound;
                })
                .Select(site => new SiteDTO
                {
                    ID = site.ID,
                    SiteURL = site.URL,
                    SiteName = site.DisplayName,
                    SSLEffectiveDate = site.SSLEffectiveDate.ToString("yyyy-MM-dd"),
                    SSLExpirationDate = site.SSLExpirationDate.ToString("yyyy-MM-dd"),
                    SSLIssuer = site.SSLIssuer,
                    SSLSubject = site.SSLSubject,
					SSLCommonName = site.SSLCommonName,
					SSLThumbprint = site.SSLThumbprint
                })
                .ToList();

                if (sitesWithinInterval.Any())
				{
					// If any reminders needed for tat interval, add a new entry to the SitesByInterval dictionary
					var intervalDto = new ReminderIntervalDTO
					{
						ID = interval.ID,
						DurationInMinutes = interval.DurationInMinutes,
						DisplayName = interval.DisplayName,
					};

					nearExpiredSites.SitesByIntervalDict[intervalDto] = sitesWithinInterval;
				}
			}

			return nearExpiredSites;
		}


		private async Task<List<RemindersToSendForSite>> GetRemindersForNearExpiredSites(NearExpiredSites nearExpiredSites)
		{
			var remindersList = new List<RemindersToSendForSite>();

			var reminders = await _healthModel.GetReminderDbSets();

			foreach (var intervalSitePair in nearExpiredSites.SitesByIntervalDict)
			{
				ReminderIntervalDTO interval = intervalSitePair.Key;
				List<SiteDTO> sitesForInterval = intervalSitePair.Value;
				
				foreach (var site in sitesForInterval)
				{
					// For each site that needs a reminder in that interval, collect any user-configured reminders that exist
					var siteReminders = reminders
						.Where(r => r.ISHealthMonitorSiteID == site.ID || r.ISHealthMonitorSiteID == 1 ) // SiteID = 1 means All Sites
						.Where(r => r.ISHealthMonitorIntervalID == interval.ID)
						.Select(r => new UserReminderDTO
						{
							ID = r.ID,
							ISHealthMonitorSiteID = r.ISHealthMonitorSiteID,
							ISHealthMonitorIntervalID = r.ISHealthMonitorIntervalID,
							ISHealthMonitorGroupSubmissionID = r.ISHealthMonitorGroupSubmissionID,
							CreatedBy = r.CreatedBy,
						})
						.ToList();

					if (siteReminders.Any())
					{
						// If there happen to be any user reminders configured, package them for sending
						remindersList.Add(new RemindersToSendForSite
						{
							ReminderInterval = intervalSitePair.Key,
							Site = site,
							Reminders = siteReminders
						});
					}
				}
			}

			return remindersList;
		}


		private List<RemindersToSendForSite> RemoveDuplicates(List<RemindersToSendForSite> siteRemindersList)
		{
			// First, flatten out all the reminders
			var flatReminders = siteRemindersList.SelectMany(sr => sr.Reminders
				.Select(r => new { SiteReminder = sr, UserReminder = r }));

			// Remove duplicates
			flatReminders = flatReminders.Distinct();

			// For each user/site pair, keep the one with the smallest interval
			var filteredReminders = flatReminders
				.GroupBy(x => (x.UserReminder.CreatedBy, x.SiteReminder.Site.ID))
				.Select(g => g.OrderBy(x => x.SiteReminder.ReminderInterval.DurationInMinutes).First())
				.ToList();

			// Group them back into the SiteReminder structure
			var filteredSiteReminders = filteredReminders
				.GroupBy(x => (x.SiteReminder.ReminderInterval, x.SiteReminder.Site))
				.Select(g => new RemindersToSendForSite
				{
					ReminderInterval = g.Key.ReminderInterval,
					Site = g.Key.Site,
					Reminders = g.Select(x => x.UserReminder).ToList()
				})
				.ToList();

			return filteredSiteReminders;
		}
	}



	internal class NearExpiredSites
    {
        public Dictionary<ReminderIntervalDTO, List<SiteDTO>> SitesByIntervalDict { get; set; }
    }

	internal class RemindersToSendForSite
	{
		public ReminderIntervalDTO ReminderInterval { get; set; }
		public SiteDTO Site { get; set; }
		public List<UserReminderDTO> Reminders { get; set; }
	}

}

