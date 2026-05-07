using System.ComponentModel.DataAnnotations;

namespace PropertyManagement.MVC.ViewModels.MaintenanceStaff
{
    public class UpdateRequestStatusViewModel
    {
        [Required(ErrorMessage = "Request ID is required")]
        public int RequestId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "New Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
    }
}