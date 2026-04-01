namespace PropertyManagement.API.Models
{
    public class Unit
    {
        public int Id { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Apartment / Office / Studio
        public double Size { get; set; }
        public decimal Rent { get; set; }
        public string Amenities { get; set; } = string.Empty;
        public string Status { get; set; } = "Available"; // Available / Occupied

        // Foreign Key
        public int BuildingId { get; set; }
        public Building Building { get; set; } = null!;

        // Navigation
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    }
}
