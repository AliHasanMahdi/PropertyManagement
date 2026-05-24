using System.ComponentModel.DataAnnotations;

namespace PropertyManagement.MVC.Models
{
    public class TrackingViewModel
    {
        [Required(ErrorMessage = "Ticket number is required")]
        [Display(Name = "Maintenance Ticket Number")]
        public string TicketNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Registered Phone Number")]
        public string Phone { get; set; } = string.Empty;

        public bool Found { get; set; } = false;
        public string? JsonResult { get; set; }
        public string? RawResult { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
