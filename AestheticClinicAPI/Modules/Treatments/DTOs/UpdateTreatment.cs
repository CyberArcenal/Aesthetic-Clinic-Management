namespace AestheticClinicAPI.Modules.Treatments.DTOs
{
    public class UpdateTreatmentDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public int? DurationMinutes { get; set; }
        public decimal? Price { get; set; }
        public bool? IsActive { get; set; }
    }
}