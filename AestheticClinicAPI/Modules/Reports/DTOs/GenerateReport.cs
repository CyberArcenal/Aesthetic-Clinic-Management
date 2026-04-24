using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Reports.DTOs
{
    public class GenerateReportDto
    {
        [Required]
        public string ReportName { get; set; } = string.Empty;

        public string? Parameters { get; set; } // JSON string of filters
    }
}