using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.Models
{
    public class QueryStructure
    {
        public string Table { get; set; }
        public List<Filter> Filters { get; set; }
        public Sort Sort { get; set; }
    }

    public class Filter
    {
        public string Column { get; set; }
        public string Operation { get; set; }
        public string Value { get; set; }
    }

    public class Sort
    {
        public string Column { get; set; }
        public string Order { get; set; }
    }

}
