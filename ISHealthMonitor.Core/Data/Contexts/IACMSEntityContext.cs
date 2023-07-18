using Microsoft.EntityFrameworkCore;
using ISHealthMonitor.Core.Data.DbSet;



namespace ISHealthMonitor.Core.Data.Contexts
{
    public partial class IACMSEntityContext : DbContext
    {
        public IACMSEntityContext()
        {
        }

        public IACMSEntityContext(DbContextOptions<IACMSEntityContext> options)
            : base(options)
        {
        }


        public DbSet<ISHealthMonitorSiteDbSet> ISHealthMonitorSites { get; set; }
        public DbSet<ISHealthMonitorIntervalDbSet> ISHealthMonitorReminderIntervals { get; set; }
        public DbSet<ISHealthMonitorUserReminderDbSet> ISHealthMonitorUserReminders { get; set; }
        public DbSet<ISHealthMonitorUserDbSet> ISHealthMonitorUsers { get; set;}
        public DbSet<ISHealthMonitorSettingDbSet> ISHealthMonitorSettings { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ISHealthMonitorUserReminderDbSet>()
				.HasOne(r => r.ISHealthMonitorSite)
				.WithMany() 
				.HasForeignKey(r => r.ISHealthMonitorSiteID);

			modelBuilder.Entity<ISHealthMonitorUserReminderDbSet>()
				.HasOne(r => r.ISHealthMonitorInterval)
				.WithMany()
				.HasForeignKey(r => r.ISHealthMonitorIntervalID);
		}

	}
}
