using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Treatments.DTOs
{
    public class CreateTreatmentDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? Category { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
    }
}