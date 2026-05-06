using System.ComponentModel.DataAnnotations;

namespace PropertyManagement.API.DTOs.Unit
{
    public class CreateUnitDto
    {
        [Required(ErrorMessage = "Unit number is required")]
        public string UnitNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; } = string.Empty;

        [Range(1, 10000, ErrorMessage = "Size must be between 1 and 10000 m²")]
        public double Size { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Rent must be greater than 0")]
        public decimal Rent { get; set; }

        public string Amenities { get; set; } = string.Empty;

        [Required(ErrorMessage = "Building is required")]
        public int BuildingId { get; set; }
    }

    public class UpdateUnitDto
    {
        [Required(ErrorMessage = "Unit number is required")]
        public string UnitNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; } = string.Empty;

        [Range(1, 10000, ErrorMessage = "Size must be between 1 and 10000 m²")]
        public double Size { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Rent must be greater than 0")]
        public decimal Rent { get; set; }

        public string Amenities { get; set; } = string.Empty;

        [Required(ErrorMessage = "Building is required")]
        public int BuildingId { get; set; }
    }

    public class UnitResponseDto
    {
        public int Id { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Size { get; set; }
        public decimal Rent { get; set; }
        public string Amenities { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
    }
}