using AestheticClinicAPI.Modules.Reports.Services;

namespace AestheticClinicAPI.BackgroundServices;

public class WeeklyReportBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<WeeklyReportBackgroundService> _logger;
    private readonly IConfiguration _config;

    public WeeklyReportBackgroundService(
        IServiceProvider services,
        ILogger<WeeklyReportBackgroundService> logger,
        IConfiguration config)
    {
        _services = services;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Basahin sa configuration kung anong araw at oras (default: Monday 2 AM UTC)
        var scheduleDay = _config.GetValue<string>("WeeklyReport:DayOfWeek") ?? "Monday";
        var scheduleHour = _config.GetValue<int>("WeeklyReport:HourUtc", 2);
        var scheduleMinute = _config.GetValue<int>("WeeklyReport:MinuteUtc", 0);

        _logger.LogInformation("WeeklyReportBackgroundService started. Schedule: every {Day} at {Hour:00}:{Minute:00} UTC", 
            scheduleDay, scheduleHour, scheduleMinute);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = GetNextRunTime(now, scheduleDay, scheduleHour, scheduleMinute);
            var delay = nextRun - now;

            _logger.LogDebug("Next weekly report scheduled at {NextRun:yyyy-MM-dd HH:mm:ss} UTC", nextRun);
            await Task.Delay(delay, stoppingToken);

            await GenerateReportAsync(stoppingToken);
        }
    }

    private async Task GenerateReportAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var aiService = scope.ServiceProvider.GetRequiredService<IAIReportingService>();
            await aiService.GenerateWeeklyReportAsync(stoppingToken);
            _logger.LogInformation("Weekly AI report generation completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weekly AI report.");
        }
    }

    private DateTime GetNextRunTime(DateTime current, string dayOfWeekStr, int hour, int minute)
    {
        if (!Enum.TryParse<DayOfWeek>(dayOfWeekStr, true, out var targetDay))
            targetDay = DayOfWeek.Monday;

        var daysUntilTarget = ((int)targetDay - (int)current.DayOfWeek + 7) % 7;
        var next = current.Date.AddDays(daysUntilTarget).AddHours(hour).AddMinutes(minute);

        if (next <= current)
            next = next.AddDays(7);

        return next;
    }
}