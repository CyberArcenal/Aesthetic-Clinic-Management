using AestheticClinicAPI.Modules.Reports.Models;

namespace AestheticClinicAPI.Modules.Reports.Services;

public interface IAIReportingService
{
    Task<ReportLog> GenerateWeeklyReportAsync(CancellationToken ct = default);
}