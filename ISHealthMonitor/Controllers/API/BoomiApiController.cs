using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.Contexts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Helpers.Email;
using ISHealthMonitor.Core.Implementations;
using ISHealthMonitor.Core.Models;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ISHealthMonitor.UI.Controllers.API
{

    [Route("api/boomi")]
    [ApiController]
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public class BoomiApiController : ControllerBase
    {
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;


		public BoomiApiController(IHealthModel healthModel, IEmployee employee)
        {
            _employee = employee;
            _healthModel = healthModel;
		}


		[HttpGet]
		[Route("refreshcerts")]
		public async Task<IActionResult> RefreshCerts()
		{
			List<ISHealthMonitorSiteDbSet> sites = await _healthModel.GetSiteDbSets();

			sites.RemoveAll(site => site.ID == 1); // Disregard the row for All Sites

			var certHandlers = new CertificateHandlers();

			var failedSiteUrls = new List<string>();

			foreach (var site in sites)
			{
				try
				{
                    CertificateDTO cert = await certHandlers.CheckSSLSiteAsync(site.URL);

                    site.SSLEffectiveDate = DateTime.Parse(cert.EffectiveDate);
                    site.SSLExpirationDate = DateTime.Parse(cert.ExpDate);
                    site.SSLIssuer = cert.Issuer;
                    site.SSLSubject = cert.Subject;

                    _healthModel.UpdateSite(site);
                }
				catch (Exception ex)
				{
					failedSiteUrls.Add(site.URL);
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


			List<EmailReminderModel> emailModels = new List<EmailReminderModel>() { };

			var nearExpiredSites = await GetNearExpiredSites();
			var remindersList = await GetRemindersForNearExpiredSites(nearExpiredSites);
			var filteredReminderList = RemoveDuplicates(remindersList);

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
                        IntervalDisplayName = siteReminders.ReminderInterval.DisplayName
                    };

                    emailModels.Add(model);
                }
                
            }

            foreach (var emailModel in emailModels)
            {
				EmailHelper.SendEmail(emailModel);
			}

            return Ok(remindersList.Count); 
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
                var lowerBound = now.Date - timeSpan;
                var upperBound = lowerBound + TimeSpan.FromDays(1);

                // Get all sites that are within the interval (need a reminder sent)
                var sitesWithinInterval = sites.Where(site =>
                {
                    var expiryDate = site.SSLExpirationDate.Date;
                    return expiryDate >= lowerBound && expiryDate < upperBound;
                })
                .Select(site => new SiteDTO
                {
                    ID = site.ID,
                    SiteURL = site.URL,
                    SiteName = site.DisplayName,
                    SSLEffectiveDate = site.SSLEffectiveDate.ToString(),
                    SSLExpirationDate = site.SSLExpirationDate.ToString(),
                    SSLIssuer = site.SSLIssuer,
                    SSLSubject = site.SSLSubject,
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
						.Where(r => r.ISHealthMonitorSiteID == site.ID && r.ISHealthMonitorIntervalID == interval.ID)
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

