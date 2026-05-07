using System.ComponentModel.DataAnnotations;

namespace PropertyManagement.MVC.ViewModels.PropertyManager
{
    public class CreateBuildingViewModel
    {
        [Required(ErrorMessage = "Building name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Building Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Display(Name = "Street Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [Display(Name = "Building Type")]
        public string Type { get; set; } = string.Empty;
    }

    public class CreateUnitViewModel
    {
        [Required(ErrorMessage = "Unit number is required")]
        [Display(Name = "Unit Number")]
        public string UnitNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [Display(Name = "Unit Type")]
        public string Type { get; set; } = string.Empty;

        [Range(1, 10000, ErrorMessage = "Size must be between 1 and 10000")]
        [Display(Name = "Size (m²)")]
        public double Size { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Rent must be greater than 0")]
        [Display(Name = "Monthly Rent (BD)")]
        public decimal Rent { get; set; }

        [Display(Name = "Amenities")]
        public string Amenities { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a building")]
        [Display(Name = "Building")]
        public int BuildingId { get; set; }
    }

    public class CreateTenantViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPR is required")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "CPR must be exactly 9 digits")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "CPR must contain only digits")]
        [Display(Name = "CPR Number")]
        public string CPR { get; set; } = string.Empty;
    }

    public class CreateLeaseViewModel
    {
        [Required(ErrorMessage = "Please select a tenant")]
        [Display(Name = "Tenant")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Please select a unit")]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

        [Range(0.01, 1000000, ErrorMessage = "Rent must be greater than 0")]
        [Display(Name = "Monthly Rent (BD)")]
        public decimal MonthlyRent { get; set; }
    }

    public class CreateStaffViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Skill type is required")]
        [Display(Name = "Skill Type")]
        public string SkillType { get; set; } = string.Empty;
    }

    public class AddPaymentViewModel
    {
        [Required(ErrorMessage = "Lease is required")]
        [Display(Name = "Lease")]
        public int LeaseId { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount (BD)")]
        public decimal Amount { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
    }
}