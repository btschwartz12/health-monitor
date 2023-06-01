using System;
using System.Collections.Generic;

namespace ISHealthMonitor.Models
{
    public class ConfigurationViewModel
    {
        public ISHealthMonitor.Core.Models.DTO.CertificateDTO certDTO { get; set; }

        public List<string> MonthNumberList { get; set; }
        public List<string> WeekNumberList { get; set; }
        public List<string> DaysNumberList { get; set; }
    }
}
