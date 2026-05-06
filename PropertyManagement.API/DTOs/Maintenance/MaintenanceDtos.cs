using System.ComponentModel.DataAnnotations;

namespace PropertyManagement.API.DTOs.Maintenance
{
    public class CreateMaintenanceRequestDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority is required")]
        public string Priority { get; set; } = "Normal";

        [Required(ErrorMessage = "Unit is required")]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Tenant is required")]
        public int TenantId { get; set; }
    }

    public class UpdateMaintenanceStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;
    }

    public class AssignStaffDto
    {
        [Required(ErrorMessage = "Staff member is required")]
        public int StaffId { get; set; }
    }

    public class MaintenanceResponseDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public string? AssignedStaffName { get; set; }
    }
}