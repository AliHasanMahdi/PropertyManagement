namespace PropertyManagement.API.Models
{
    public class Building
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Residential / Commercial

        // Navigation
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
    }
}
