using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Helpers.WorkOrder
{
    public class WorkOrderModel
    {
        private readonly ILogger<object> _logger;

        public WorkOrderModel(ILogger<object> logger)
        {
            _logger = logger;
        }


        public List<AttrInfo> CreateAttrList(int objectid, int workOrderCategory,
                                             int systemProfile, string title,
                                             string description, string urgency,
                                             string reasonForEmergency,
                                             string origin)
        {
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
        

        public OnbaseWorkviewObjectDTO GetWorkViewObjectDTO(int objectid, string appName,
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
