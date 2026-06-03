namespace PropertyManagement.API.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        // Optional due date for the payment. Nullable to preserve compatibility
        // with existing records that don't have a due date saved.
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "Pending"; 
        public string Notes { get; set; } = string.Empty;

        // Foreign Key
        public int LeaseId { get; set; }
        public Lease Lease { get; set; } = null!;
    }
}
