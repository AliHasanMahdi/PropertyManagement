namespace PropertyManagement.API.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CPR { get; set; } = string.Empty;
        public DateTime DateRegistered { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
