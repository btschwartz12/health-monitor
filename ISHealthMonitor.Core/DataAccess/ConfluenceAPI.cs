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
        public string? ServiceAccountToken { get; set; }
        public string? ServiceAccountI { get; set; }

    }
    public class Body
    {
        public string representation { get; set; }
        public string value { get; set; }
    }

    public class ConfluenceAPIPage
    {
        public int id { get; set; }
        public string status { get; set; }
        public string title { get; set; }
        public int spaceId { get; set; }
        public Body body { get; set; }
        public Version version { get; set; }
    }

    public class Version
    {
        public int number { get; set; }
        public string message { get; set; }
    }
    public class BodyStorage
    {
        public Storage storage { get; set; }
    }

    public class Links
    {
        public string editui { get; set; }
        public string webui { get; set; }
        public string tinyui { get; set; }
    }

    public class ConfluencePageInfo
    {
        public int id { get; set; }
        public Version version { get; set; }
        public string parentType { get; set; }
        public string authorId { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public BodyStorage body { get; set; }
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

   

}
