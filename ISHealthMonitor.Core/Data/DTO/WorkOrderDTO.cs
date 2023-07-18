using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DTO
{
    public class WorkOrderDTO
    {
        [Required(ErrorMessage = "IssueType is required")]
        [Display(Name = "Issue Type")]
        public string IssueType { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Required(ErrorMessage = "System is required")]
        [Display(Name = "System")]
        public string System { get; set; }

        [Required(ErrorMessage = "Urgency is required")]
        [Display(Name = "Urgency")]
        public string Urgency { get; set; }

        [Required(ErrorMessage = "Short Description is required")]
        [Display(Name = "Short Description")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Emergency Reason is required")]
        [Display(Name = "Emergency Reason")]
        public string EmergencyReason { get; set; }

        public string? SiteName { get; set; }
        public string? SiteURL { get; set; }
        public int SiteID { get; set; }
    }

    public class OnbaseWorkviewObjectDTO
    {
        public long objectId { get; set; }
        public string appName { get; set; }
        public string className { get; set; }
        public List<AttrInfo> attributeList { get; set; }
        public bool appIdSpecified { get; set; }
        public bool appNameSpecified { get; set; }
        public bool classIdSpecified { get; set; }
        public bool classNameSpecified { get; set; }
        public string email { get; set; }
        public string newRank { get; set; }
    }
    public class AttrInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<string> value { get; set; }
        public bool idSpecified { get; set; }
        public bool nameSpecified { get; set; }
    }

    public class ExpandedWorkviewObject
    {
        public long Objectid { get; set; }
        public bool IsCardSearch { get; set; }
        public bool IsCardRanked { get; set; }
        public Dictionary<string, object> Values { get; set; }
    }

    public class Constraint
    {
        [JsonProperty("DottedAddress")]
        public string DottedAddress { get; set; }

        [JsonProperty("Operator")]
        public string Operator { get; set; }

        [JsonProperty("Value")]
        public object Value { get; set; }

        [JsonProperty("Connector")]
        public object? Connector { get; set; }

        [JsonProperty("Grouping")]
        public object? Grouping { get; set; }

        [JsonProperty("WorkviewOperator")]
        public int WorkviewOperator { get; set; }

        [JsonProperty("WorkviewGrouping")]
        public object? WorkviewGrouping { get; set; }

        [JsonProperty("WorkviewConnector")]
        public object? WorkviewConnector { get; set; }
    }

    public class RequestBody
    {
        [JsonProperty("Constraints")]
        public List<Constraint> Constraints { get; set; }
    }


    public static class WorkOrderModel
    {

        public static List<AttrInfo> CreateAttrList(int objectid, int workOrderCategory,
                                             int systemProfile, string title,
                                             string description, string urgency,
                                             string reasonForEmergency,
                                             string origin)
        {
            if (reasonForEmergency == "null")
            {
                reasonForEmergency = "";
            }

            var attrList = new List<AttrInfo>
            {
                new AttrInfo()
                {
                    name = "Requestor",
                    nameSpecified = true,
                    value = new List<string>() { objectid.ToString() },
                },
                new AttrInfo()
                {
                    name = "LinkToWorkOrderCategory",
                    nameSpecified = true,
                    value = new List<string>() { workOrderCategory.ToString() },
                },
                new AttrInfo()
                {
                    name = "LinkToSystemProfile",
                    nameSpecified = true,
                    value = new List<string>() { systemProfile.ToString() },
                },
                new AttrInfo()
                {
                    name = "Title",
                    nameSpecified = true,
                    value = new List<string>() { title },
                },
                new AttrInfo()
                {
                    name = "Content",
                    nameSpecified = true,
                    value = new List<string>() { description },
                },
                new AttrInfo()
                {
                    name = "Urgency",
                    nameSpecified = true,
                    value = new List<string>() { urgency },
                },
                new AttrInfo()
                {
                    name = "ReasonForEmergency",
                    nameSpecified = true,
                    value = new List<string>() { reasonForEmergency },
                },
                new AttrInfo()
                {
                    name = "Origin",
                    nameSpecified = true,
                    value = new List<string>() { origin },
                },
            };

            return attrList;
        }


        public static OnbaseWorkviewObjectDTO GetWorkViewObjectDTO(int objectid, string appName,
                                                            string className, List<AttrInfo> attrList)
        {
            var wvObject = new OnbaseWorkviewObjectDTO()
            {
                objectId = objectid,
                appName = appName,
                appNameSpecified = true,
                className = className,
                classNameSpecified = true,
                attributeList = attrList,

            };

            return wvObject;
        }
    }



}
