namespace AestheticClinicAPI.Modules.Reports.DTOs
{
    public class ReportLogResponseDto
    {
        public int Id { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string? Parameters { get; set; }
        public int? GeneratedById { get; set; }
        public string? GeneratedByName { get; set; }
        public string? Insights { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}