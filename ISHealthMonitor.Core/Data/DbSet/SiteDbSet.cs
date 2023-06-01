using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DbSet
{
    public class SiteDbSet
    {
            public int ID { get; set; }
            public bool Active { get; set; }
            public System.Guid CreatedBy { get; set; }
            public System.DateTime DateCreated { get; set; }
            public System.DateTime DateModified { get; set; }
            public bool Deleted { get; set; }
            public string? DisplayName { get; set; }
      
    }
}
