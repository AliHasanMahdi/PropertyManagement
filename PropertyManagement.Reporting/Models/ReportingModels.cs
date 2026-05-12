namespace PropertyManagement.Reporting.Models
{
    public class OccupancyReport
    {
        public int TotalUnits { get; set; }
        public int OccupiedUnits { get; set; }
        public int AvailableUnits { get; set; }
        public double OccupancyRate { get; set; }
    }

    public class MaintenanceStats
    {
        public int TotalRequests { get; set; }
        public int ResolvedRequests { get; set; }
        public int PendingRequests { get; set; }
    }

    public class LeaseModel
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

    public class MaintenanceModel
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public string? AssignedStaffName { get; set; }
    }

    public class BuildingModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int TotalUnits { get; set; }
        public int AvailableUnits { get; set; }
    }

    public class DashboardViewModel
    {
        public OccupancyReport? Occupancy { get; set; }
        public MaintenanceStats? MaintenanceStats { get; set; }
        public List<LeaseModel> OverdueLeases { get; set; } = new();
        public List<MaintenanceModel> PendingRequests { get; set; } = new();
        public List<BuildingModel> Buildings { get; set; } = new();
    }
}