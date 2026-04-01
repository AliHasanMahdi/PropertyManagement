namespace PropertyManagement.API.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Keys (nullable - goes to either Tenant or Staff)
        public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public int? MaintenanceStaffId { get; set; }
        public MaintenanceStaff? MaintenanceStaff { get; set; }
    }
}
