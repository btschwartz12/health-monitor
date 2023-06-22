using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.Contexts;
using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Data.Models;
using ISHealthMonitor.Core.Models.DTO;
using Microsoft.EntityFrameworkCore;
using ISHealthMonitor.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Models
{
    public class HealthModel : IHealthModel
    {
		private readonly IACMSEntityContext _IACMSEntityContext;

        public HealthModel(IACMSEntityContext context)
        {
            _IACMSEntityContext = context;
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
							  Action =	"<div class='text-center'><i style='cursor: pointer;' class='fa fa-pencil fa-lg text-primary mr-3' " +
										"onclick=showSiteAddEditModal(" + d.ID + ")></i><i style='cursor: pointer;' class='fa fa-trash fa-lg " +
										" text-danger' onclick=showSiteDeleteModal(" + d.ID + ")></i></div>"
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
			return new SiteDTO()
			{
				ID = site.ID,
				SiteURL = site.URL,
				SiteName= site.DisplayName,
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
                ID= interval.ID,
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

		public CertificateDTO GetCertificate(string url)
		{
			throw new NotImplementedException();
		}

        public async Task<List<ISHealthMonitorSiteDbSet>> GetSiteDbSets()
        {
			List<ISHealthMonitorSiteDbSet> sites = await _IACMSEntityContext.ISHealthMonitorSites.Where(r => !r.Deleted && !r.Disabled).ToListAsync();

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
	}
}

