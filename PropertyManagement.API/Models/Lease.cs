namespace PropertyManagement.API.Models
{
    public class Lease
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Status { get; set; } = "Application";
        // Application → Screening → Approved → Active → Renewal / Terminated

        // Foreign Keys
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        // Navigation
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
