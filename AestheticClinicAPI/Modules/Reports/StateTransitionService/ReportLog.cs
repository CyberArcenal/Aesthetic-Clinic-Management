using AestheticClinicAPI.Modules.Reports.Models;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Reports.StateTransitionService;

public class ReportLogStateTransition : IStateTransitionService<ReportLog>
{
    private readonly ILogger<ReportLogStateTransition> _logger;
    private readonly IUserService _userService;
    private readonly INotifyLogService _notifyLogService;

    public ReportLogStateTransition(
        ILogger<ReportLogStateTransition> logger,
        IUserService userService,
        INotifyLogService notifyLogService)
    {
        _logger = logger;
        _userService = userService;
        _notifyLogService = notifyLogService;
    }

    public async Task OnCreatedAsync(ReportLog reportLog, CancellationToken ct = default)
    {
        _logger.LogInformation("[REPORT] New report generated: '{ReportName}' (ID: {Id}) at {GeneratedAt}",
            reportLog.ReportName, reportLog.Id, reportLog.GeneratedAt);

        // Get all admin users
        var adminUsersResult = await _userService.GetAllAsync();
        if (!adminUsersResult.IsSuccess || adminUsersResult.Data == null)
        {
            _logger.LogWarning("Could not retrieve admin users for report notification.");
            return;
        }

        var adminEmails = adminUsersResult.Data
            .Where(u => u.Roles != null && u.Roles.Contains("Admin"))
            .Select(u => u.Email)
            .Where(e => !string.IsNullOrEmpty(e))
            .ToList();

        if (!adminEmails.Any())
        {
            _logger.LogWarning("No admin emails found to send report notification.");
            return;
        }

        // Prepare email content
        var subject = $"New Report Generated: {reportLog.ReportName}";
        var body = $@"
Report Name: {reportLog.ReportName}
Generated At: {reportLog.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC

Parameters: {reportLog.Parameters ?? "None"}

Insights:
{reportLog.Insights ?? "No insights available yet."}

Please log into the admin dashboard to view the full report.
";

        // Send email to each admin
        foreach (var email in adminEmails)
        {
            await _notifyLogService.CreateAsync(new QueueNotificationDto
            {
                Recipient = email,
                Channel = "Email",
                Type = "custom",  // or create a template named "NewReportNotification"
                Subject = subject,
                Message = body,
                Metadata = new Dictionary<string, string>
                {
                    { "ReportName", reportLog.ReportName },
                    { "GeneratedAt", reportLog.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "Parameters", reportLog.Parameters ?? "None" },
                    { "Insights", reportLog.Insights ?? "No insights yet" }
                }
            });
        }

        _logger.LogInformation("Sent report notification email to {Count} admin(s)", adminEmails.Count);
    }

    public Task OnUpdatedAsync(ReportLog reportLog, ReportLog? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[REPORT] Report log {Id} updated", reportLog.Id);
        if (originalEntity != null && string.IsNullOrEmpty(originalEntity.Insights) && !string.IsNullOrEmpty(reportLog.Insights))
        {
            _logger.LogInformation("   → AI insights added to report '{ReportName}'", reportLog.ReportName);
            // Optionally send another notification that insights are ready
        }
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(ReportLog reportLog, string oldStatus, string newStatus, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task OnActiveChangedAsync(ReportLog entity, bool oldActive, bool newActive, CancellationToken ct = default)
        => Task.CompletedTask;
}