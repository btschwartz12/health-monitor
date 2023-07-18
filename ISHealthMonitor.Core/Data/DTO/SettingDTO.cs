using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Data.DTO
{
    public class SettingDTO
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "DisplayName is required")]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "Value is required")]
        [Display(Name = "Value")]
        public string Value { get; set; }

        [Display(Name = "Action")]
        public string Action { get; set; }
    }
}
