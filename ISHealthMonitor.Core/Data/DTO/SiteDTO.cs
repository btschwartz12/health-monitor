using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Models.DTO
{
    public class SiteDTO
    {
        [Key]
        public int id { get; set; }
        public string SiteName { get; set; }
        public string SiteURL { get; set; }
       
        public string Action { get; set; }
    }
}
