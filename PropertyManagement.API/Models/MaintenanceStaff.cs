namespace PropertyManagement.API.Models
{
    public class MaintenanceStaff
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string SkillType { get; set; } = string.Empty; // Plumbing / Electrical / etc
        public string AvailabilityStatus { get; set; } = "Available"; // Available / Busy

        // Navigation
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
