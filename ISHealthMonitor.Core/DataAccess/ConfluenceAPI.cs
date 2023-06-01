using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.DataAccess
{
    public class ConfluenceAPI
    {
        public string? APIToken { get; set; }
        public string? Endpoint { get; set; }
        public string? ServiceAccount { get; set; }
    }
}
