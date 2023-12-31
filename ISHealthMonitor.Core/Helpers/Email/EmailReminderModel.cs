﻿using ISHealthMonitor.Core.Data.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Helpers.Email
{
    public class EmailReminderModel
    {
        public List<string> Emails { get; set; }
        public int SiteID { get; set; }
        public string SiteURL { get; set; }
        public string SiteName { get; set; }
        public string SSLEffectiveDate { get; set; }
        public string SSLExpirationDate { get; set; }
        public string SSLIssuer { get; set; }
        public string SSLSubject { get; set; }
        public string SSLThumbprint { get; set; }
        public string SSLCommonName { get; set; }
        public string IntervalDisplayName { get; set; }
        public string Notes { get; set; }



		public readonly string TemplateRelativePath = "wwwroot\\lib\\templates\\EmailTemplate.html";

		public readonly string Subject = "Site Certificate Expiration Warning";
		
        public readonly string From = "noresponse-health-monitor@Onbase.com";
	}
}
