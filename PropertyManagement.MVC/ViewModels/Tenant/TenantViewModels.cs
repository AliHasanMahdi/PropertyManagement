using System.ComponentModel.DataAnnotations;

namespace PropertyManagement.MVC.ViewModels.Tenant
{
    public class CreateMaintenanceRequestViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Request Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Normal";

        [Required(ErrorMessage = "Please select a unit")]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }
    }
}