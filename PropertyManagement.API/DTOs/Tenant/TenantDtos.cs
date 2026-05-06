using System.ComponentModel.DataAnnotations;

namespace PropertyManagement.API.DTOs.Tenant
{
    public class CreateTenantDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPR is required")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "CPR must be exactly 9 digits")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "CPR must contain only digits")]
        public string CPR { get; set; } = string.Empty;
    }

    public class UpdateTenantDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;
    }

    public class TenantResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CPR { get; set; } = string.Empty;
        public DateTime DateRegistered { get; set; }
        public int TotalLeases { get; set; }
        public bool HasActiveLease { get; set; }
    }
}