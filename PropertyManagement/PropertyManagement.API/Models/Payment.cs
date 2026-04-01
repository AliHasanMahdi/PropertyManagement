namespace PropertyManagement.API.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending / Paid / Overdue
        public string Notes { get; set; } = string.Empty;

        // Foreign Key
        public int LeaseId { get; set; }
        public Lease Lease { get; set; } = null!;
    }
}
