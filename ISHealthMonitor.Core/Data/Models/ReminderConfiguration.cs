using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Data.DTO;
using System.Collections.Generic;

namespace ISHealthMonitor.Core.Data.Models
{
    public class ReminderConfiguration
    {
        public int GroupID { get; set; }
        public List<UserReminderDTO> UserReminders { get; set; }
        public string? DateCreated { get; set; }
    }
}
