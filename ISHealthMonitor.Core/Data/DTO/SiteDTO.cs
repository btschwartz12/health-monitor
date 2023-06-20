using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DTO
{
	public class SiteDTO
	{
		[Key]
		public int ID { get; set; }

		[Required(ErrorMessage = "SiteURL is required")]
		[Display(Name = "Site URL")]
		public string? SiteURL { get; set; }

		[Required(ErrorMessage = "SiteName is required")]
		[Display(Name = "Site Name")]
		public string? SiteName { get; set; }

        [Required(ErrorMessage = "SiteCategory is required")]
        [Display(Name = "Site Category")]
        public string? SiteCategory { get; set; }

        [Required(ErrorMessage = "SSLEffectiveDate is required")]
		[Display(Name = "SSL Effective Date")]
		public string? SSLEffectiveDate { get; set; }

		[Required(ErrorMessage = "SSLExpirationDate is required")]
		[Display(Name = "SSL Expiration Date")]
		public string? SSLExpirationDate { get; set; }

		[Required(ErrorMessage = "SSLIssuer is required")]
		[Display(Name = "SSL Issuer")]
		public string? SSLIssuer { get; set; }

		[Required(ErrorMessage = "SSLSubject is required")]
		[Display(Name = "SSL Subject")]
		public string? SSLSubject { get; set; }

		[Required(ErrorMessage = "SSLCommonName is required")]
		[Display(Name = "SSL Common Name")]
		public string? SSLCommonName { get; set; }

        [Required(ErrorMessage = "SSLThumbprint is required")]
        [Display(Name = "SSL Thumbprint")]
        public string? SSLThumbprint { get; set; }

        public string? Action { get; set; }
	}

}
