using System.ComponentModel.DataAnnotations;

namespace PropertyManagement.API.DTOs.Lease
{
    public class CreateLeaseDto
    {
        [Required(ErrorMessage = "Tenant is required")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Monthly rent must be greater than 0")]
        public decimal MonthlyRent { get; set; }
    }

    public class UpdateLeaseStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;
    }

    public class AddPaymentDto
    {
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public string Notes { get; set; } = string.Empty;
    }

    public class LeaseResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public string BuildingName { get; set; } = string.Empty;
        public decimal TotalPaid { get; set; }
        public bool HasOverduePayments { get; set; }
    }
}