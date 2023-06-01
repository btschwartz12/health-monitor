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

        public DbSet<SiteDbSet> Site { get; set; }
      
    }
}
