using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Reports.Models;
using Microsoft.EntityFrameworkCore;

namespace AestheticClinicAPI.Modules.Reports.Services;

public class AIReportingService : IAIReportingService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AIReportingService> _logger;

    public AIReportingService(AppDbContext dbContext, ILogger<AIReportingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ReportLog> GenerateWeeklyReportAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting weekly AI prediction report generation at {Time}", DateTime.UtcNow);

        // TODO: Dito mo ilalagay ang actual AI prediction logic.
        // Halimbawa:
        // - Kunin ang data ng appointments, clients, treatments mula _dbContext
        // - Tawagin ang external AI model (HTTP, ML.NET, etc.)
        // - I-format ang resulta bilang insights

        // Placeholder: simulate na AI processing
        await Task.Delay(3000, ct); // 3 seconds delay

        var insights = @"
            Weekly AI Prediction Report:
            - Predicted number of new clients next week: 12
            - Most requested treatment: HydraFacial (expected +25% demand)
            - Revenue forecast: ₱85,000 - ₱95,000
            - Staff utilization: 82% (recommend adding 1 more specialist on Saturdays)
        ";

        var parameters = $"{{\"generationDate\": \"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\", \"week\": \"{GetCurrentWeekNumber()}\"}}";

        var report = new ReportLog
        {
            ReportName = "WeeklyAIPrediction",
            Parameters = parameters,
            GeneratedById = null, // system-generated, walang user ID
            Insights = insights,
            GeneratedAt = DateTime.UtcNow
        };

        _dbContext.ReportLogs.Add(report);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Weekly AI report saved with ID {ReportId}", report.Id);
        return report;
    }

    private int GetCurrentWeekNumber()
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(DateTime.UtcNow, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}