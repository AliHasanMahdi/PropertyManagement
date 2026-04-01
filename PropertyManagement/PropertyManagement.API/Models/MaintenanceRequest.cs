namespace PropertyManagement.API.Models
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Plumbing / Electrical / etc
        public string Priority { get; set; } = "Normal"; // Low / Normal / High / Urgent
        public string Status { get; set; } = "Submitted";
        // Submitted → Assigned → InProgress → Resolved → Closed
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ResolvedAt { get; set; }

        // Foreign Keys
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        public int? MaintenanceStaffId { get; set; }
        public MaintenanceStaff? MaintenanceStaff { get; set; }
    }
}
