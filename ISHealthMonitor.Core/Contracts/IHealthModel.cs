using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using ISHealthMonitor.Core.Models.DTO;
using ISHealthMonitor.Core.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ISHealthMonitor.Core.Models;

namespace ISHealthMonitor.Core.Contracts
{
    public interface IHealthModel
    {

		// Sites
		//List<CertificateDTO> GetSites();
        List<SiteDTO> GetSites();
		Task<List<ISHealthMonitorSiteDbSet>> GetSiteDbSets();
        ISHealthMonitorSiteDbSet GetSite(int id);
		SiteDTO GetSiteDTO(int id);
        void AddSite(ISHealthMonitorSiteDbSet site);
        void UpdateSite(ISHealthMonitorSiteDbSet site);
		void DeleteSite(int id);
		public List<string> GetSubscribedUsersForSite(int siteId);

		// Reminders
		List<UserReminderDTO> GetReminders();
		Task<List<ISHealthMonitorUserReminderDbSet>> GetReminderDbSets();
		ISHealthMonitorUserReminderDbSet GetReminder(int id);
		void AddReminder(ISHealthMonitorUserReminderDbSet reminder);
		void UpdateReminder(ISHealthMonitorUserReminderDbSet reminder);
		void DeleteReminder(int id);
		int GetNextReminderGroupID();
		void DeleteReminderGroup(int id);
		void DeleteRemindersBySite(Guid user, int id);



		// Reminder Intervals
		List<ReminderIntervalDTO> GetReminderIntervals();
        Task<List<ISHealthMonitorIntervalDbSet>> GetReminderIntervalDbSets();
        ISHealthMonitorIntervalDbSet GetReminderInterval(int id);
        ReminderIntervalDTO GetReminderIntervalDTO(int id);
        void AddReminderInterval(ISHealthMonitorIntervalDbSet reminderInterval);
		void UpdateReminderInterval(ISHealthMonitorIntervalDbSet reminderInterval);
		void DeleteReminderInterval(int id);
		public List<string> GetSubscribedUsersForInterval(int intervalId);


		// Users
		List<UserDTO> GetUsers();
		ISHealthMonitorUserDbSet GetUser(int id);
		bool AddUser(ISHealthMonitorUserDbSet user);
		void UpdateUser(ISHealthMonitorUserDbSet user);
		void DeleteUser(int id);
		bool UserIsAdmin(Guid guid);
		bool UserExists(Guid guid);
		Task<bool> UserHasReminders(Guid guid);


		// Settings

		List<SettingDTO> GetSettings();
		ISHealthMonitorSettingDbSet GetSetting(int id);
		SettingDTO GetSettingDTO(int id);
		void AddSetting(ISHealthMonitorSettingDbSet setting);
		void UpdateSetting(ISHealthMonitorSettingDbSet setting);
		void DeleteSetting(int id);

		string? GetSettingValue(string key);



		DateTime GetCreatedDateForGroup(int groupId);


		List<SiteReminderConfiguration> GetSiteReminderConfigurations(Guid guid);
		List<ReminderConfigurationData> GetReminderConfigurationsData(Guid guid, int siteID);


		// Boomi functions

		Task<(string Message, Dictionary<string, string> FailedSiteUrls)> UpdateCerts();
		Task<(string Message, Dictionary<string, string> responseData)> UpdateConfluencePage();
		Task<(string Message, Dictionary<string, Dictionary<string, List<string>>> remindersSent)> FireReminders();
        Task<(string Message, List<Dictionary<string, string>> workOrdersAttempted, List<Dictionary<string, string>> sitesWithExistingWorkOrders)> AutoCreateWorkOrders(EmployeeDTO requestor);


        // Helpers for FireReminders()

        Task<List<RemindersToSendForSite>> GetRemindersForNearExpiredSites(NearExpiredSites nearExpiredSites);
		Task<NearExpiredSites> GetNearExpiredSites();
		List<RemindersToSendForSite> RemoveDuplicates(List<RemindersToSendForSite> siteRemindersList);

		string GetTimeDiffString(DateTime expDate);
		string GetTimeDiffColor(DateTime expDate, string? commonName);

		Dictionary<string, List<string>> GetSubscriptionsForSite(int siteId);
		int GetNumSubscriptionsForSite(int siteId);


		void UpdateWorkOrderForSite(int siteId, int workOrderObjectId);
        Task<Dictionary<string, string>> CreateWorkOrder(WorkOrderDTO workOrderDTO, EmployeeDTO employee);








    }

	public class NearExpiredSites
	{
		public Dictionary<ReminderIntervalDTO, List<SiteDTO>> SitesByIntervalDict { get; set; }
	}

	public class RemindersToSendForSite
	{
		public ReminderIntervalDTO ReminderInterval { get; set; }
		public SiteDTO Site { get; set; }
		public List<UserReminderDTO> Reminders { get; set; }
	}
}
