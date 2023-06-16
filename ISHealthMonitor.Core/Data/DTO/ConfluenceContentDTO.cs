using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DTO
{
    public class Body
    {
        public Storage storage { get; set; }
    }

    public class Links
    {
        public string editui { get; set; }
        public string webui { get; set; }
        public string tinyui { get; set; }
    }

    public class ConfluenceContent
    {
        public int id { get; set; }
        public Version version { get; set; }
        public string parentType { get; set; }
        public string authorId { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public Body body { get; set; }
        public int parentId { get; set; }
        public int spaceId { get; set; }
        public DateTime createdAt { get; set; }
        public Links _links { get; set; }
    }

    public class Storage
    {
        public string value { get; set; }
        public string representation { get; set; }
    }

    public class Version
    {
        public int number { get; set; }
        public string message { get; set; }
        public bool minorEdit { get; set; }
        public string authorId { get; set; }
        public DateTime createdAt { get; set; }
    }
}
