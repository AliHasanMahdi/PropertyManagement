using System.ComponentModel.DataAnnotations;

namespace PropertyManagement.MVC.Models
{
    public class TrackingViewModel
    {
        [Required]
        [Display(Name = "Reference")]
        public string Reference { get; set; } = string.Empty;

        public bool Found { get; set; } = false;
        public string? JsonResult { get; set; }
        public string? RawResult { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
