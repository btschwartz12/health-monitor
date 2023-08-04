using Microsoft.AspNetCore.Mvc;
using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.Contexts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Data.Models;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ISHealthMonitor.Core.Common;
using ISHealthMonitor.Core.DataAccess;
using ISHealthMonitor.Core.Helpers.Confluence;
using ISHealthMonitor.Core.Helpers.Email;
using Microsoft.Extensions.Logging;
using ISHealthMonitor.Core.Implementations;
using System.Security.Policy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ISHealthMonitor.Core.Model;
using System.Text.Json;
using System;

namespace ISHealthMonitor.Core.Models
{
    public class HealthModel : IHealthModel
	{
		private readonly IACMSEntityContext _IACMSEntityContext;
		private readonly IEmployee _employee;
		private readonly IRest _restModel;
		private readonly IConfiguration _config;
		private readonly ILogger<HealthModel> _logger;

		public HealthModel(IACMSEntityContext context, IEmployee employee, IRest rest, IConfiguration configuration, ILogger<HealthModel> logger)
		{
			_IACMSEntityContext = context;
			_employee = employee;
			_restModel = rest;
			_config = configuration;
			_logger = logger;
		}



		public List<SiteDTO> GetSites()
		{
			var sites = _IACMSEntityContext.ISHealthMonitorSites.Where(r => !r.Deleted).ToList();

			var result = (from d in sites
						  select new SiteDTO()
						  {
							  ID = d.ID,
							  SiteURL = d.URL,
							  SiteName = d.DisplayName,
							  SiteCategory = d.SiteCategory,
							  SSLEffectiveDate = d.SSLEffectiveDate.ToString("yyyy-MM-dd"),
							  SSLExpirationDate = d.SSLExpirationDate.ToString("yyyy-MM-dd"),
							  SSLIssuer = d.SSLIssuer,
							  SSLSubject = d.SSLSubject,
							  SSLCommonName = d.SSLCommonName,
							  SSLThumbprint = d.SSLThumbprint,
							  Action = "<div class='text-center'><i style='cursor: pointer;' class='fa fa-pencil fa-lg text-primary mr-3' " +
										"onclick=showSiteAddEditModal(" + d.ID + ")></i><i style='cursor: pointer;' class='fa fa-trash fa-lg " +
										" text-danger' onclick=showSiteDeleteModal(" + d.ID + ")></i></div>",
							  Notes = d.Notes,
							  AllowWorkOrderCreation = d.AllowWorkOrderCreation
						  })
				  .ToList();

			return result;
		}

		public ISHealthMonitorSiteDbSet GetSite(int id)
		{
			ISHealthMonitorSiteDbSet? site = _IACMSEntityContext.ISHealthMonitorSites.FirstOrDefault(x => x.ID == id);
			return site;
		}

		public SiteDTO GetSiteDTO(int id)
		{
			if (id == 0)
			{
				return new SiteDTO() { ID = 0 };
			}
			ISHealthMonitorSiteDbSet site = GetSite(id);

            if (site == null)
			{
				throw new Exception("Site does not exist with id: " + id);
			}

			return new SiteDTO()
			{
				ID = site.ID,
				SiteURL = site.URL,
				SiteName = site.DisplayName,
				SiteCategory = site.SiteCategory,
				SSLEffectiveDate = site.SSLEffectiveDate.ToString("yyyy-MM-dd"),
				SSLExpirationDate = site.SSLExpirationDate.ToString("yyyy-MM-dd"),
				SSLIssuer = site.SSLIssuer,
				SSLSubject = site.SSLSubject,
				SSLCommonName = site.SSLCommonName,
				SSLThumbprint = site.SSLThumbprint
			};
		}

		public void AddSite(ISHealthMonitorSiteDbSet site)
		{
			_IACMSEntityContext.Add(site);
            _IACMSEntityContext.SaveChanges();
		}

		public void UpdateSite(ISHealthMonitorSiteDbSet site)
		{
			_IACMSEntityContext.Entry(site).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();
		}

		public void DeleteSite(int id)
		{

			// Assuming GetUsersWithRemindersForSite() has already made sure no reminders for this site exist

			ISHealthMonitorSiteDbSet? record = _IACMSEntityContext.ISHealthMonitorSites.FirstOrDefault(s => s.ID == id);
			record.Deleted = true;
			_IACMSEntityContext.Entry(record).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();
		}

		public List<string> GetSubscribedUsersForSite(int siteId)
		{
			var activeRemindersForSite = _IACMSEntityContext.ISHealthMonitorUserReminders
											.Where(r => !r.Deleted)
											.Where(r => r.ISHealthMonitorSiteID == siteId)
											.ToList();
			var userNameList = activeRemindersForSite.Select(r => r.UserName).Distinct().ToList();

			return userNameList;
		}


		public List<UserReminderDTO> GetReminders()
		{
			var reminders = _IACMSEntityContext.ISHealthMonitorUserReminders.Where(r => !r.Deleted).ToList();

			var result = (from d in reminders
						  select new UserReminderDTO()
						  {
							  ID = d.ID,
							  UserName = d.UserName,
							  ISHealthMonitorSiteID = d.ISHealthMonitorSiteID,
							  ISHealthMonitorIntervalID = d.ISHealthMonitorIntervalID,
							  ISHealthMonitorGroupSubmissionID = d.ISHealthMonitorGroupSubmissionID,
							  
							  Site = GetSiteDTO(d.ISHealthMonitorSiteID),
							  ReminderInterval = GetReminderIntervalDTO(d.ISHealthMonitorIntervalID),
							  Action = "<div class='text-center'><i style='cursor: pointer;' class='fa fa-pencil fa-lg text-primary mr-3' " +
										"onclick=showReminderAddEditModal(" + d.ID + ")></i><i style='cursor: pointer;' class='fa fa-trash fa-lg " +
										" text-danger' onclick=showReminderDeleteModal(" + d.ID + ")></i></div>"
						  })
						  .ToList();

			return result;
		}

		public ISHealthMonitorUserReminderDbSet GetReminder(int id)
		{
			var reminder = _IACMSEntityContext.ISHealthMonitorUserReminders.FirstOrDefault(r => r.ID == id);
			return reminder;
		}

		public void AddReminder(ISHealthMonitorUserReminderDbSet reminder)
		{
			_IACMSEntityContext.ISHealthMonitorUserReminders.Add(reminder);
			_IACMSEntityContext.SaveChanges();
		}

		public void UpdateReminder(ISHealthMonitorUserReminderDbSet reminder)
		{
			_IACMSEntityContext.Entry(reminder).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();
		}

		public void DeleteReminder(int id)
		{
			var reminder = _IACMSEntityContext.ISHealthMonitorUserReminders.FirstOrDefault(r => r.ID == id);
			reminder.Deleted = true;
			_IACMSEntityContext.Entry(reminder).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();
		}

		public List<ReminderIntervalDTO> GetReminderIntervals()
		{
			var intervals = _IACMSEntityContext.ISHealthMonitorReminderIntervals.Where(r => !r.Deleted).ToList();
			var result = intervals.Select(d => new ReminderIntervalDTO()
			{
				ID = d.ID,
				DurationInMinutes = d.DurationInMinutes,
				Type = d.Type,
				DisplayName = d.DisplayName,
				Action = "<div class='text-center'><i style='cursor: pointer;' class='fa fa-pencil fa-lg text-primary mr-3' " +
						"onclick=showIntervalAddEditModal(" + d.ID + ")></i><i style='cursor: pointer;' class='fa fa-trash fa-lg " +
						" text-danger' onclick=showIntervalDeleteModal(" + d.ID + ")></i></div>"
			}).ToList();
			return result;
		}

		public ISHealthMonitorIntervalDbSet GetReminderInterval(int id)
		{
			var interval = _IACMSEntityContext.ISHealthMonitorReminderIntervals.FirstOrDefault(i => i.ID == id);
			return interval;
		}

		public ReminderIntervalDTO GetReminderIntervalDTO(int id)
		{
			ISHealthMonitorIntervalDbSet interval = GetReminderInterval(id);
			return new ReminderIntervalDTO()
			{
				ID = interval.ID,
				DurationInMinutes = interval.DurationInMinutes,
				Type = interval.Type,
				DisplayName = interval.DisplayName,
			};
		}

		public void AddReminderInterval(ISHealthMonitorIntervalDbSet reminderInterval)
		{
			_IACMSEntityContext.ISHealthMonitorReminderIntervals.Add(reminderInterval);
			_IACMSEntityContext.SaveChanges();
		}

		public void UpdateReminderInterval(ISHealthMonitorIntervalDbSet reminderInterval)
		{
			_IACMSEntityContext.Entry(reminderInterval).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();
		}

		public void DeleteReminderInterval(int id)
		{
			var interval = _IACMSEntityContext.ISHealthMonitorReminderIntervals.FirstOrDefault(i => i.ID == id);
			interval.Deleted = true;
			_IACMSEntityContext.Entry(interval).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();
		}

		public List<UserDTO> GetUsers()
		{
			var users = _IACMSEntityContext.ISHealthMonitorUsers.Where(r => !r.Deleted).ToList();
			var result = users.Select(d => new UserDTO()
			{
				ID = d.ID,
				DisplayName = d.DisplayName,
				IsAdmin = d.IsAdmin,
				Action = "<div class='text-center'><i style='cursor: pointer;' class='fa fa-pencil fa-lg text-primary mr-3' " +
						$"onclick=showUserAddEditModal({d.ID})></i><i style='cursor: pointer;' class='fa fa-trash fa-lg " +
						$"text-danger' onclick=showUserDeleteModal({d.ID})></i></div>"
			}).ToList();
			return result;
		}

		public ISHealthMonitorUserDbSet GetUser(int id)
		{
			var user = _IACMSEntityContext.ISHealthMonitorUsers.FirstOrDefault(u => u.ID == id);
			return user;
		}

		public bool AddUser(ISHealthMonitorUserDbSet user)
		{
			if (UserExists(user.Guid))
			{
				return false;
			}

			_IACMSEntityContext.ISHealthMonitorUsers.Add(user);
			_IACMSEntityContext.SaveChanges();

			return true;
		}

		public bool UserExists(Guid guid)
		{
			var existingUser = _IACMSEntityContext.ISHealthMonitorUsers.Where(r => !r.Deleted).FirstOrDefault(u => u.Guid == guid);
			return existingUser != null;
		}

		public void UpdateUser(ISHealthMonitorUserDbSet user)
		{
			_IACMSEntityContext.Entry(user).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();
		}

		public void DeleteUser(int id)
		{
			var user = _IACMSEntityContext.ISHealthMonitorUsers.FirstOrDefault(u => u.ID == id);
			user.Deleted = true;
			_IACMSEntityContext.Entry(user).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();
		}

		public bool UserIsAdmin(Guid guid)
		{
			ISHealthMonitorUserDbSet? existingUser = _IACMSEntityContext.ISHealthMonitorUsers.Where(r => !r.Deleted).FirstOrDefault(u => u.Guid == guid);

			if (existingUser == null) { return false; }

			return existingUser.IsAdmin;
		}



		public int GetNextReminderGroupID()
		{
			var highestGroupId = _IACMSEntityContext.ISHealthMonitorUserReminders
				.Where(r => !r.Deleted)
				.OrderByDescending(r => r.ISHealthMonitorGroupSubmissionID)
				.FirstOrDefault()?.ISHealthMonitorGroupSubmissionID;

			if (highestGroupId.HasValue)
			{
				return highestGroupId.Value + 1;
			}
			else
			{
				return 1;
			}
		}



		public List<SiteReminderConfiguration> GetSiteReminderConfigurations(Guid user_guid)
		{
			var userReminders = _IACMSEntityContext.ISHealthMonitorUserReminders
				.Where(r => r.CreatedBy == user_guid && !r.Deleted)
				.ToList();

			var siteReminders = new List<SiteReminderConfiguration>();

			var groupedUserReminders = userReminders.GroupBy(r => r.ISHealthMonitorSiteID);

			foreach (var group in groupedUserReminders)
			{
				var site = GetSiteDTO(group.Key);

				var reminderList = new List<ReminderIntervalDTO>();
				var uniqueIds = new HashSet<int>();

				foreach (var reminder in group)
				{
					if (!uniqueIds.Contains(reminder.ISHealthMonitorIntervalID))
					{
						var interval = GetReminderIntervalDTO(reminder.ISHealthMonitorIntervalID);
						reminderList.Add(interval);
						uniqueIds.Add(reminder.ISHealthMonitorIntervalID);
					}
				}
				reminderList = reminderList.OrderBy(r => r.DurationInMinutes).ToList();
				var msg = "This feature is not yet implemented.";
				var siteReminder = new SiteReminderConfiguration
				{
					ISHealthMonitorSiteID = site.ID,
					SiteName = site.SiteName,
					ReminderIntervals = reminderList,
					Action = $"<div class='text-center'><i style='cursor: pointer;' class='fa fa-pencil fa-lg text-primary mr-3' " +
					$"onclick='redirectToConfigurationHistory({site.ID})'></i><i style='cursor: pointer;' class='fa fa-trash fa-lg " +
					$"text-danger' onclick='showSiteConfigurationDeleteModal({site.ID})'></i></div>"

				};

				siteReminders.Add(siteReminder);
			}

			return siteReminders;
		}

		public List<ReminderConfigurationData> GetReminderConfigurationsData(Guid user_guid, int siteID)
		{
			var userReminders = _IACMSEntityContext.ISHealthMonitorUserReminders
				.Include(r => r.ISHealthMonitorSite) // Include site data
				.Include(r => r.ISHealthMonitorInterval) // Include interval data
				.Where(r => r.CreatedBy == user_guid && !r.Deleted)
				.ToList();

			var groupedReminders = userReminders
				.GroupBy(r => r.ISHealthMonitorGroupSubmissionID)
				.Where(g => siteID == 0 || g.Any(r => r.ISHealthMonitorSite.ID == siteID)) // Apply filter based on siteID
				.Select(g =>
				{
					var siteToRemindersMap = g.GroupBy(r => r.ISHealthMonitorSite.DisplayName)
						.ToDictionary(grp => grp.Key, grp => grp.Select(r => r.ISHealthMonitorInterval.DisplayName).ToList());

					return new ReminderConfigurationData
					{
						GroupID = g.Key,
						NumReminders = g.Count(),
						CreatedDate = g.Min(r => r.CreatedDate).ToString("yyyy-MM-dd"), // Get the earliest created date
						ConfiguredSiteNames = g.Select(r => r.ISHealthMonitorSite.DisplayName).Distinct().ToList(), // Distinct to avoid duplicate site names
						Action = "<div class='text-center'><i style='cursor: pointer;' class='fa fa-pencil fa-lg text-primary mr-3' " +
							$"onclick='redirectToConfigurationBuilder({g.Key})'></i><i style='cursor: pointer;' class='fa fa-trash fa-lg " +
							$"text-danger' onclick=showConfigurationFormDeleteModal({g.Key})></i></div>",
						SiteToRemindersMap = siteToRemindersMap
					};
				})
				.ToList();

			return groupedReminders;
		}


		public void DeleteReminderGroup(int groupId)
		{
			var remindersInGroup = _IACMSEntityContext.ISHealthMonitorUserReminders
										.Where(r => r.ISHealthMonitorGroupSubmissionID == groupId);

			foreach (var reminder in remindersInGroup)
			{
				reminder.Deleted = true;
				_IACMSEntityContext.Entry(reminder).State = EntityState.Modified;
			}

			_IACMSEntityContext.SaveChanges();
		}






		public void DeleteRemindersBySite(Guid user, int id)
		{
			var remindersForSite = _IACMSEntityContext.ISHealthMonitorUserReminders
										.Where(r => r.CreatedBy == user)
										.Where(r => r.ISHealthMonitorSiteID == id);

			foreach (var reminder in remindersForSite)
			{
				reminder.Deleted = true;
				_IACMSEntityContext.Entry(reminder).State = EntityState.Modified;
			}

			_IACMSEntityContext.SaveChanges();
		}

		public List<string> GetSubscribedUsersForInterval(int intervalId)
		{
			var activeRemindersForInterval = _IACMSEntityContext.ISHealthMonitorUserReminders
									.Where(r => !r.Deleted)
									.Where(r => r.ISHealthMonitorIntervalID == intervalId)
									.ToList();
			var userNameList = activeRemindersForInterval.Select(r => r.UserName).Distinct().ToList();

			return userNameList;
		}



		public async Task<List<ISHealthMonitorSiteDbSet>> GetSiteDbSets()
		{
			List<ISHealthMonitorSiteDbSet> sites = await _IACMSEntityContext.ISHealthMonitorSites.Where(r => !r.Deleted).ToListAsync();

			return sites;

		}

		public async Task<List<ISHealthMonitorUserReminderDbSet>> GetReminderDbSets()
		{
			var reminders = await _IACMSEntityContext.ISHealthMonitorUserReminders.Where(r => !r.Deleted).ToListAsync();

			return reminders;
		}

		public async Task<List<ISHealthMonitorIntervalDbSet>> GetReminderIntervalDbSets()
		{
			List<ISHealthMonitorIntervalDbSet> intervals = await _IACMSEntityContext.ISHealthMonitorReminderIntervals.Where(r => !r.Deleted).ToListAsync();

			return intervals;
		}

		public async Task<bool> UserHasReminders(Guid guid)
		{
			var reminders = await _IACMSEntityContext.ISHealthMonitorUserReminders
				.Where(r => !r.Deleted)
				.Where(r => r.CreatedBy == guid)
				.ToListAsync();

			return reminders.Count > 0;
		}

		public async Task<(string Message, Dictionary<string, string> FailedSiteUrls)> UpdateCerts()
		{
			List<ISHealthMonitorSiteDbSet> sites = await GetSiteDbSets();

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
					site.LastUpdated = DateTime.Now;


                    if (cert.ErrorCommonName)
					{
						site.SSLCommonName = "INVALID (" + cert.CommonName + ")";
                        throw new Exception("Invalid SSL Common Name for the site.");

					}

                    // Mark as modified so we can save later
                    _IACMSEntityContext.Entry(site).State = EntityState.Modified;

                }
				catch (Exception ex)
				{
                    // Mark as modified so we can save later
                    _IACMSEntityContext.Entry(site).State = EntityState.Modified;

                    failedSiteUrls.Add(site.URL, ex.Message);
				}

			}

			// Save all changes to the db
            _IACMSEntityContext.SaveChanges();

            if (failedSiteUrls.Count > 0)
			{
				return ("Success", failedSiteUrls); // Still success, even if some sites dont have certs 
			}
			else
			{
				return ("Success", new Dictionary<string, string>());
			}
		}

		public async Task<(string Message, Dictionary<string, string> responseData)> UpdateConfluencePage()
		{
			List<ConfluenceSiteRowModel> sites = GetSites()
				.Where(site => site.SiteCategory != "All" && site.SiteCategory != "Test")
				.OrderBy(site => site.SiteName)
				.Select(site =>
				{
					var workOrderData = GetPendingWorkOrderData(site.ID);
					bool pendingWorkOrder = workOrderData["Exists"] == "true";
					
					var workOrderURL = "";
					
					if (pendingWorkOrder)
					{
						workOrderURL = workOrderData["URL"].Replace("&", "&amp;");
					}
					

					return new ConfluenceSiteRowModel
					{
						ID = site.ID,
						SiteURL = site.SiteURL,
						SiteName = site.SiteName,
						SSLEffectiveDate = site.SSLEffectiveDate,
						SSLExpirationDate = site.SSLExpirationDate,
						SSLIssuer = site.SSLIssuer,
						SSLSubject = site.SSLSubject,
						SSLCommonName = site.SSLCommonName,
						SSLThumbprint = site.SSLThumbprint,
						TimeUntilExpiration = GetTimeDiffString(DateTime.Parse(site.SSLExpirationDate)),
						RowColor = GetTimeDiffColor(DateTime.Parse(site.SSLExpirationDate), site.SSLCommonName),
						PendingWorkOrder = pendingWorkOrder,
						WorkOrderURL = workOrderURL,
						//WorkOrderURL = "https://example.com?bob=job%26joe=burrow",
						WorkOrderSubmittedDate = pendingWorkOrder ? workOrderData["SubmittedDate"] : ""
					};
				})
				.ToList();

			Dictionary<string, string> colors = GetColors();

			Dictionary<string, int> thresholds = GetThresholds();

			ConfluenceTableModel model = new ConfluenceTableModel()
			{
				sites = sites,
				Colors = colors,
				Thresholds = thresholds
			};



			string rootDir = _config.GetSection("TemplatePaths")["ConfluenceTable"];

			string wikiEndpoint = _config.GetSection("ConfluenceCloudApp")["WikiEndpoint"];

			int confluencePageID = int.Parse(_config.GetSection("ConfluenceCloudApp")["PageID"]);
			int confluenceSpaceID = int.Parse(_config.GetSection("ConfluenceCloudApp")["SpaceID"]);
			string confluencePageTitle = _config.GetSection("ConfluenceCloudApp")["PageTitle"];


			var tableStr = ConfluenceTableHelper.GetSiteTableHTML(model, rootDir);

			//tableStr = "test";


			//example to get Page Source
			var action = wikiEndpoint + "?body-format=storage";
			var rek = await _restModel.GetHttpContent(action);

			var confluencePage = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfluencePageInfo>(rek);

			int curVersion = confluencePage.version.number;


			//example to Update Page
			var pp = new ConfluenceAPIPage()
			{
				id = confluencePageID,
				spaceId = confluenceSpaceID,
				title = confluencePageTitle,
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
			//{ "errors":[{ "status":400,"code":"INVALID_REQUEST_PARAMETER","title":"Content body cannot be converted to new editor format","detail":null}]}
			try
			{
				var rekt = await _restModel.PutHttpContent(wikiEndpoint, Newtonsoft.Json.JsonConvert.SerializeObject(pp), HttpContentTypes.String);

				JObject resp = JsonConvert.DeserializeObject<JObject>(rekt);
				if (resp.ContainsKey("errors"))
				{
					throw new Exception(rekt.ToString());
				}


				return ("Success", new Dictionary<string, string>
				{
					{"version", (curVersion + 1).ToString() },
					{"numSites", sites.Count.ToString() }
				});
			}
			catch (Exception ex)
			{
				return ("Failure", new Dictionary<string, string>
				{
					{"errorMsg", ex.ToString() },
				});
			}
		}

		public async Task<(string Message, Dictionary<string, Dictionary<string, List<string>>> remindersSent)> FireReminders()
		{
			string rootDir = _config.GetSection("TemplatePaths")["EmailReminder"];

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
						SiteID = siteReminders.Site.ID,
						SiteURL = siteReminders.Site.SiteURL,
						SiteName = siteReminders.Site.SiteName,
						SSLEffectiveDate = siteReminders.Site.SSLEffectiveDate,
						SSLExpirationDate = siteReminders.Site.SSLExpirationDate,
						SSLIssuer = siteReminders.Site.SSLIssuer,
						SSLSubject = siteReminders.Site.SSLSubject,
						SSLCommonName = siteReminders.Site.SSLCommonName,
						SSLThumbprint = siteReminders.Site.SSLThumbprint,
						IntervalDisplayName = siteReminders.ReminderInterval.DisplayName,
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

			var remindersSent = new Dictionary<string, Dictionary<string, List<string>>>();

			foreach (var reminder in filteredReminderList)
			{
				string siteUrl = reminder.Site.SiteURL;
				string intervalDisplayName = reminder.ReminderInterval.DisplayName;

				if (!remindersSent.ContainsKey(siteUrl))
				{
					remindersSent[siteUrl] = new Dictionary<string, List<string>>();
				}

				if (!remindersSent[siteUrl].ContainsKey(intervalDisplayName))
				{
					remindersSent[siteUrl][intervalDisplayName] = new List<string>();
				}

				foreach (var userReminder in reminder.Reminders)
				{
					remindersSent[siteUrl][intervalDisplayName].Add(userReminder.CreatedBy.ToString());
                }
            }


            return ("Success", remindersSent);
		}

		public async Task<(string Message, List<Dictionary<string, string>> workOrdersAttempted, List<Dictionary<string, string>> sitesWithExistingWorkOrders)> AutoCreateWorkOrders(EmployeeDTO requestor)
		{
            var workOrdersAttempted = new List<Dictionary<string, string>>() { };
            var sitesWithExistingWorkOrders = new List<Dictionary<string, string>>() { };

			bool creationEnabled = true;
			string respMessage = "Success";

            var workOrderBaseUrl = _config.GetSection("UnityRestAPI")["HSIDBWorkOrderURL"];


            var enabledStatus = GetSettingValue("autoWorkOrderEnabled");
            var threshold = new TimeSpan();

            if (enabledStatus == null || enabledStatus != "true")
			{
				creationEnabled = false;
				respMessage = "Disabled";
            }
            else if (int.TryParse(GetSettingValue("autoWorkOrderThresholdDays"), out int days))
            {
                threshold = TimeSpan.FromDays(days);
            }
            else
            {
                creationEnabled = false;
                respMessage = "Invalid Threshold";
            }

            DateTime thresholdDate = DateTime.Now + threshold;

            List<ISHealthMonitorSiteDbSet> allSites = _IACMSEntityContext.ISHealthMonitorSites
													.Where(s => !s.Deleted && s.ID != 1)
													.ToList();


            foreach (var site in allSites)
            {
                if (site.HSIDBWorkOrderLastSubmittedDate != null && site.SSLEffectiveDate < site.HSIDBWorkOrderLastSubmittedDate)
				{
					sitesWithExistingWorkOrders.Add(new Dictionary<string, string>
					{
						{ "siteName", site.DisplayName },
						{ "siteURL", site.URL },
						{ "sslEffectiveDate", site.SSLEffectiveDate.ToShortDateString() },
                        { "sslExpirationDate", site.SSLExpirationDate.ToShortDateString() },
                        { "workOrderSubmittedDate", site.HSIDBWorkOrderLastSubmittedDate?.ToShortDateString() ?? "N/A" },
                        { "workOrderObjectID", site.HSIDBWorkOrderCurrentObjectID?.ToString() ?? "N/A" },
                        { "workOrderURL", (workOrderBaseUrl + site.HSIDBWorkOrderCurrentObjectID?.ToString()) ?? "N/A"},
                    });
				}
				else if (creationEnabled && site.SSLExpirationDate < thresholdDate)
				{
                    WorkOrderDTO model = new WorkOrderDTO()
                    {
                        SiteID = site.ID,
                        SiteName = site.DisplayName,
                        SiteURL = site.URL,
                        IssueType = "Other",
                        Category = "Help Desk",
                        System = "Help Desk",
                        ShortDescription = $"Update certificate for {site.DisplayName}",
                        Urgency = "2",
                        Description = $"The SSL Certificate is going to expire on: {site.SSLExpirationDate} for the site URL: {site.URL}"
                    };

					Dictionary<string, string> resp = await CreateWorkOrder(model, requestor);

                    if (resp["Message"] == "Success")
                    {
                        var objectId = int.Parse(resp["ObjectID"]);
						workOrdersAttempted.Add(new Dictionary<string, string>
						{
							{ "result", "Success" },
                            { "siteName", site.DisplayName },
							{ "siteURL", site.URL },
							{ "sslEffectiveDate", site.SSLEffectiveDate.ToShortDateString() },
							{ "sslExpirationDate", site.SSLExpirationDate.ToShortDateString() },
                            { "workOrderObjectID", objectId.ToString() },
							{ "workOrderURL", workOrderBaseUrl + objectId.ToString() }
						});
                    }
                    else if (resp["Message"] == "Failed")
                    {
                        workOrdersAttempted.Add(new Dictionary<string, string>
                        {
                            { "result", "Failed" },
                            { "siteName", site.DisplayName },
                            { "siteURL", site.URL },
                            { "sslEffectiveDate", site.SSLEffectiveDate.ToShortDateString() },
                            { "sslExpirationDate", site.SSLExpirationDate.ToShortDateString() },
                            { "errorReason", resp["Description"] }
                        });
                    }

                }
            }
            return (respMessage, workOrdersAttempted, sitesWithExistingWorkOrders);
        }

        public async Task<Dictionary<string, string>> CreateWorkOrder(WorkOrderDTO model, EmployeeDTO employee)
		{
			
			var res = new Dictionary<string, string>() { };


			SiteDTO site = GetSiteDTO(model.SiteID);

			if (!site.AllowWorkOrderCreation)
			{
				res.Add("Message", "Failed");
				res.Add("Description", "Unauthorized");
				return res;
			}

			var unityModel = new UnityRestAPIAccess(_logger, _config);

            int objectid;

		

            try
            {
				objectid = await unityModel.GetRequestorId(employee.Email);

                if (objectid == -1)
                {
                    throw new Exception("Response does not contain a valid objectid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get requestor id for ({employee.Email}): {ex}");
				res.Add("Message", "Failed");
				res.Add("Description", ex.Message);
				return res;
            }

	

			int linkToWorkOrderCategory = int.Parse(_config.GetSection("UnityRestAPI")["WOCategoryID"]);
			int linkToSystemProfile = int.Parse(_config.GetSection("UnityRestAPI")["WOSystemProfile"]);
			string origin = _config.GetSection("UnityRestAPI")["WOOrigin"];
			string appName = _config.GetSection("UnityRestAPI")["WOAppName"];
			string className = _config.GetSection("UnityRestAPI")["WOClassName"];
			int workOrderObjId = 0;

            var attrList = WorkOrderModel.CreateAttrList(objectid, linkToWorkOrderCategory,
                                                         linkToSystemProfile, model.ShortDescription,
                                                         model.Description, model.Urgency,
                                                         model.EmergencyReason, origin);




            OnbaseWorkviewObjectDTO wvObject = WorkOrderModel.GetWorkViewObjectDTO(workOrderObjId, appName,
                                                                                    className, attrList);

            var unityApi = new UnityRestAPIAccess(_logger, _config);

            try
            {
                string wvObjectJson = System.Text.Json.JsonSerializer.Serialize(wvObject);

                var objectId = await unityApi.CreateWorkViewObject(wvObject.appName, wvObject.className,
                                                        wvObjectJson);

                UpdateWorkOrderForSite(model.SiteID, int.Parse(objectId));

                _logger.LogInformation($"Work Order created by {employee.GUID} for siteID={model.SiteID}. Work Order object ID = {objectId}");
                res.Add("Message", "Success");
                res.Add("ObjectID", objectId.ToString());
				return res;
            }
            catch (Exception ex)
            {
				_logger.LogError($"Failed to create work order: {ex}");
                res.Add("Message", "Failed");
                res.Add("Description", ex.Message);
				return res;
            }

		}



        public async Task<NearExpiredSites> GetNearExpiredSites()
		{
			var nearExpiredSites = new NearExpiredSites
			{
				SitesByIntervalDict = new Dictionary<ReminderIntervalDTO, List<SiteDTO>>()
			};

			var now = DateTime.Now;
			List<ISHealthMonitorSiteDbSet> sites = await GetSiteDbSets();
			List<ISHealthMonitorIntervalDbSet> intervals = await GetReminderIntervalDbSets();

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


		public async Task<List<RemindersToSendForSite>> GetRemindersForNearExpiredSites(NearExpiredSites nearExpiredSites)
		{
			var remindersList = new List<RemindersToSendForSite>();

			var reminders = await GetReminderDbSets();

			foreach (var intervalSitePair in nearExpiredSites.SitesByIntervalDict)
			{
				ReminderIntervalDTO interval = intervalSitePair.Key;
				List<SiteDTO> sitesForInterval = intervalSitePair.Value;

				foreach (var site in sitesForInterval)
				{
					// For each site that needs a reminder in that interval, collect any user-configured reminders that exist
					var siteReminders = reminders
						.Where(r => r.ISHealthMonitorSiteID == site.ID || r.ISHealthMonitorSiteID == 1) // SiteID = 1 means All Sites
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


		public List<RemindersToSendForSite> RemoveDuplicates(List<RemindersToSendForSite> siteRemindersList)
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

		public DateTime GetCreatedDateForGroup(int groupId)
		{
			var reminderGroup = _IACMSEntityContext.ISHealthMonitorUserReminders
								.Where(r => !r.Deleted)
								.Where(r => r.ISHealthMonitorGroupSubmissionID == groupId)
								.ToList();

			var minTime = reminderGroup.Min(r => r.CreatedDate);

			return minTime;
		}

		public string GetTimeDiffString(DateTime expDate)
		{
			TimeSpan timeDiff = expDate - DateTime.Now;

			string timeDiffReadable = "";
			if (timeDiff.TotalSeconds < 0)
			{
				timeDiffReadable = "Expired";
			}
			else
			{
				int years = timeDiff.Days / 365; // get the number of years
				int months = (timeDiff.Days % 365) / 30; // get the number of remaining months
				int days = (timeDiff.Days % 365) % 30; // get the number of remaining days

				if (years > 0)
				{
					timeDiffReadable += $"{(years > 0 ? $"{years} year{(years != 1 ? "s" : "")}, " : "")}";
				}
				if (months > 0)
				{
					timeDiffReadable += $"{(months > 0 ? $"{months} month{(months != 1 ? "s" : "")}, " : "")}";
				}
				timeDiffReadable += $"{days} day{(days != 1 ? "s" : "")}";
			}
			return timeDiffReadable;
		}

		public string GetTimeDiffColor(DateTime expDate, string? commonName)
		{
			TimeSpan timeDiff = expDate - DateTime.Now;

			var colors = GetColors();
			string RED_COLOR = colors["RED_COLOR"];
			string YELLOW_COLOR = colors["YELLOW_COLOR"];
			string GREEN_COLOR = colors["GREEN_COLOR"];
			string GRAY_COLOR = colors["GRAY_COLOR"];
			string NO_COLOR = colors["NO_COLOR"];

			var thresholds = GetThresholds();
			int redThreshold = thresholds["RED_THRESHOLD"];
			int yellowThreshold = thresholds["YELLOW_THRESHOLD"];
			int greenThreshold = thresholds["GREEN_THRESHOLD"];



			if (timeDiff.TotalDays <= redThreshold)
			{
				return RED_COLOR;
			}
			
			if (timeDiff.TotalDays <= yellowThreshold)
			{
				return YELLOW_COLOR; // orange
			}
			if (commonName != null && commonName.StartsWith("INVALID"))
			{
				return GRAY_COLOR;
			}
			if (timeDiff.TotalDays > greenThreshold)
			{
				return GREEN_COLOR;
			}

			return NO_COLOR;
		}


		public Dictionary<string, int> GetThresholds()
		{
			string redThresholdSetting = GetSettingValue("redExpirationStatusMaxDaysAway");
			string yellowThresholdSetting = GetSettingValue("yellowExpirationStatusMaxDaysAway");
			string greenThresholdSetting = GetSettingValue("greenExpirationStatusMinDaysAway");
			int redThreshold, yellowThreshold, greenThreshold;

			if (!int.TryParse(redThresholdSetting, out redThreshold))
			{
				_logger.LogError("Invalid red threshold");
				redThreshold = 10;
			}
			if (!int.TryParse(yellowThresholdSetting, out yellowThreshold))
			{
				_logger.LogError("Invalid yellow threshold");

				yellowThreshold = 30;
			}

			if (!int.TryParse(greenThresholdSetting, out greenThreshold))
			{
				_logger.LogError("Invalid green threshold");
				greenThreshold = 365;
			}

			return new Dictionary<string, int>
			{
				{"RED_THRESHOLD", redThreshold},
				{"YELLOW_THRESHOLD", yellowThreshold},
				{"GREEN_THRESHOLD", greenThreshold }
			};
		}


		public Dictionary<string, string> GetColors()
		{
			return new Dictionary<string, string>
			{
				{"RED_COLOR", "#ff8f73"},
				{"YELLOW_COLOR", "#faff94"},
				{"GREEN_COLOR", "#b3ffab"},
				{"GRAY_COLOR", "#ede4d8"}, // #fce4e3
				{"NO_COLOR", ""}
			};
		}





		public Dictionary<string, List<string>> GetSubscriptionsForSite(int siteId)
        {
            SiteDTO site = GetSites()
				.Where(s => s.ID == siteId)
				.First();

			if (site == null)
			{
				throw new Exception("Site does not exist");
			}

			// All reminders that exist
            List<UserReminderDTO> allReminders = GetReminders();

			// Reminders for just 'All Sites'
            List<UserReminderDTO> remindersForAllSites = allReminders
                .Where(r => r.ISHealthMonitorSiteID == 1)
                .ToList();

			// Get all reminder intervals for sorting and displaying in a list
            List<ReminderIntervalDTO> allReminderIntervals = GetReminderIntervals();
            Dictionary<int, (int, string)> reminderIntervalDictionary = allReminderIntervals.ToDictionary(x => x.ID, x => (x.DurationInMinutes, x.DisplayName));

			// Get reminders specific to the site
            List<UserReminderDTO> remindersForSite = allReminders
                        .Where(r => r.ISHealthMonitorSiteID == site.ID)
                        .ToList();

            Dictionary<string, List<string>> usersSubscribed = new Dictionary<string, List<string>>();

            Dictionary<string, List<(int, string)>> usersSubscribedTemp = new Dictionary<string, List<(int, string)>>();

            foreach (var reminder in remindersForSite)
            {
                int duration = reminderIntervalDictionary[reminder.ISHealthMonitorIntervalID].Item1;
                string displayName = reminderIntervalDictionary[reminder.ISHealthMonitorIntervalID].Item2;

                // If the user already has reminders, add to their list. Otherwise, create a new list.
                if (usersSubscribedTemp.ContainsKey(reminder.UserName))
                {
                    usersSubscribedTemp[reminder.UserName].Add((duration, displayName));
                }
                else
                {
                    usersSubscribedTemp[reminder.UserName] = new List<(int, string)> { (duration, displayName) };
                }

            }

            // Do seperate stuff for all sites so that it is differentiable
            foreach (var allSiteReminder in remindersForAllSites)
            {
                int duration = reminderIntervalDictionary[allSiteReminder.ISHealthMonitorIntervalID].Item1;
                string displayName = reminderIntervalDictionary[allSiteReminder.ISHealthMonitorIntervalID].Item2;

                if (usersSubscribedTemp.ContainsKey(allSiteReminder.UserName))
                {
                    usersSubscribedTemp[allSiteReminder.UserName].Add((duration, displayName + " (All Sites)"));
                }
                else
                {
                    usersSubscribedTemp[allSiteReminder.UserName] = new List<(int, string)> { (duration, displayName + " (All Sites)") };
                }
            }

            // Sort the lists by duration and convert to lists of display names.
            foreach (var userName in usersSubscribedTemp.Keys)
            {
                List<string> sortedDisplayNames = usersSubscribedTemp[userName]
                    .OrderBy(x => x.Item1)
                    .Select(x => x.Item2)
                    .ToList();

                usersSubscribed[userName] = sortedDisplayNames;
            }

            return usersSubscribed;
        }

        public void UpdateWorkOrderForSite(int siteId, int workOrderObjectId)
        {
			ISHealthMonitorSiteDbSet site = GetSite(siteId);

			site.HSIDBWorkOrderCurrentObjectID = workOrderObjectId;
			site.HSIDBWorkOrderLastSubmittedDate = DateTime.Now;

			_IACMSEntityContext.Entry(site).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();


        }

        public int GetNumSubscriptionsForSite(int siteId)
        {
            SiteDTO site = GetSites()
               .Where(s => s.ID == siteId)
               .First();

            if (site == null)
            {
				return -1;
            }

            int numSubscribers = _IACMSEntityContext.ISHealthMonitorUserReminders
								 .Where(r => !r.Deleted && (r.ISHealthMonitorSiteID == siteId || r.ISHealthMonitorSiteID == 1))
								 .GroupBy(r => r.CreatedBy)
								 .Count();

            return numSubscribers;




        }

		public List<SettingDTO> GetSettings()
		{
			var settings = _IACMSEntityContext.ISHealthMonitorSettings.Where(s => !s.Deleted).ToList();
			var result = settings.Select(d => new SettingDTO()
			{
				ID = d.ID,
				Name = d.Name,
				DisplayName = d.DisplayName,
				Value = d.Value,
				Action = "<div class='text-center'><i style='cursor: pointer;' class='fa fa-pencil fa-lg text-primary mr-3' " +
						"onclick=showSettingAddEditModal(" + d.ID + ")></i><i style='cursor: pointer;' class='fa fa-trash fa-lg " +
						" text-danger' onclick=showSettingDeleteModal(" + d.ID + ")></i></div>"
			}).ToList();
			return result;
		}

		public ISHealthMonitorSettingDbSet GetSetting(int id)
		{
			var setting = _IACMSEntityContext.ISHealthMonitorSettings.FirstOrDefault(i => i.ID == id);
			return setting;
		}

		public SettingDTO GetSettingDTO(int id)
		{
			ISHealthMonitorSettingDbSet setting = GetSetting(id);
			return new SettingDTO()
			{
				ID = setting.ID,
				Name = setting.Name,
				DisplayName = setting.DisplayName,
				Value = setting.Value,
			};
		}

		public void AddSetting(ISHealthMonitorSettingDbSet setting)
		{
			_IACMSEntityContext.ISHealthMonitorSettings.Add(setting);
			_IACMSEntityContext.SaveChanges();
		}

		public void UpdateSetting(ISHealthMonitorSettingDbSet setting)
		{
			_IACMSEntityContext.Entry(setting).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();
		}

		public void DeleteSetting(int id)
		{
			var setting = _IACMSEntityContext.ISHealthMonitorSettings.FirstOrDefault(i => i.ID == id);
			setting.Deleted = true;
			_IACMSEntityContext.Entry(setting).State = EntityState.Modified;
			_IACMSEntityContext.SaveChanges();
		}


		public string? GetSettingValue(string key)
		{
			var setting = _IACMSEntityContext.ISHealthMonitorSettings.FirstOrDefault(s => s.Name == key);
			if (setting == null)
			{
				return null;
			}
			return setting.Value;
		}

		public Dictionary<string, string> GetPendingWorkOrderData(int siteId)
		{
			ISHealthMonitorSiteDbSet site = GetSite(siteId);

			var res = new Dictionary<string, string>()
			{
				{"Exists", "false"}
			};

			if (site == null)
			{
				return res;
			}

			if (site.HSIDBWorkOrderCurrentObjectID == null || site.HSIDBWorkOrderLastSubmittedDate == null)
			{
				return res;
			}

			if (site.HSIDBWorkOrderLastSubmittedDate <= site.SSLEffectiveDate)
			{
				return res;
			}

			var workOrderBaseUrl = _config.GetSection("UnityRestAPI")["HSIDBWorkOrderURL"];

			res["Exists"] = "true";
			res.Add("URL", workOrderBaseUrl + site.HSIDBWorkOrderCurrentObjectID);
			res.Add("SubmittedDate", site.HSIDBWorkOrderLastSubmittedDate.ToString());

			return res;
		}
	}
}


