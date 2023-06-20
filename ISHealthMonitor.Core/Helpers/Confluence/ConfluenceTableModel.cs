using ISHealthMonitor.Core.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Helpers.Confluence
{
    public class ConfluenceTableModel
    {
        public List<SiteDTO> sites;

        public readonly string TemplatePath = "C:\\Users\\bschwartz\\source\\repos\\ishealthmonitor\\ISHealthMonitor.Core\\Helpers\\Confluence\\ConfluenceTableTemplate.cshtml";
    }
}
