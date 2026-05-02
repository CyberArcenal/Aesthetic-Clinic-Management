using AestheticClinicAPI.Modules.Reports.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Reports.StateTransitionService;

public class ReportLogStateTransition : IStateTransitionService<ReportLog>
{
    private readonly ILogger<ReportLogStateTransition> _logger;

    public ReportLogStateTransition(ILogger<ReportLogStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(ReportLog reportLog, CancellationToken ct = default)
    {
        _logger.LogInformation("[REPORT] New report generated: '{ReportName}' (ID: {Id}) by UserId: {GeneratedById} at {GeneratedAt}",
            reportLog.ReportName, reportLog.Id, reportLog.GeneratedById, reportLog.GeneratedAt);
        
        // TODO:
        // - I-save ang report file (PDF/CSV) sa storage
        // - I-queue ang AI analysis para sa insights (kung wala pa)
        // - I-notify ang user na ready na ang report
        // - I-log sa audit trail
        // - Mag-send ng email attachment kung kinakailangan
        
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(ReportLog reportLog, ReportLog? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[REPORT] Report log {Id} updated", reportLog.Id);
        
        if (originalEntity != null)
        {
            // Kung nagkaroon ng insights (na-generate ng AI)
            if (string.IsNullOrEmpty(originalEntity.Insights) && !string.IsNullOrEmpty(reportLog.Insights))
            {
                _logger.LogInformation("   → AI insights added to report '{ReportName}'", reportLog.ReportName);
                // TODO: i-save ang insights sa separate file, i-notify ang user
            }
            
            // Kung nagbago ang parameters (filters)
            if (originalEntity.Parameters != reportLog.Parameters)
            {
                _logger.LogInformation("   → Report parameters changed from '{OldParams}' to '{NewParams}'", 
                    originalEntity.Parameters, reportLog.Parameters);
                // TODO: re-generate ang report kung kinakailangan
            }
        }
        
        return Task.CompletedTask;
    }

    // Walang Status property si ReportLog, placeholder lang
    public Task OnStatusChangedAsync(ReportLog reportLog, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogDebug("[REPORT] OnStatusChangedAsync called but ReportLog has no Status field. Ignoring.");
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(ReportLog entity, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}