using AestheticClinicAPI.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Reports.Models
{
    public class ReportLog : BaseEntity
    {
        [Required, MaxLength(100)]
        public string ReportName { get; set; } = string.Empty;  // e.g., "WeeklySales", "InventoryReport"

        public string? Parameters { get; set; }  // JSON string of filters (e.g., { "startDate": "2025-01-01", "endDate": "2025-01-31" })

        public int? GeneratedById { get; set; }  // User ID who generated the report

        public string? Insights { get; set; }    // AI-generated summary or findings

        [Required]
        public DateTime GeneratedAt { get; set; }  // when the report was generated
    }
}