using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Notifications.StateTransitionService;

public class NotifyLogStateTransition : IStateTransitionService<NotifyLog>
{
    private readonly ILogger<NotifyLogStateTransition> _logger;

    public NotifyLogStateTransition(ILogger<NotifyLogStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(NotifyLog log, CancellationToken ct = default)
    {
        _logger.LogInformation("[NOTIFYLOG] New notification log: Recipient {RecipientEmail}, Type {Type}, Channel {Channel}, Status {Status}",
            log.RecipientEmail, log.Type, log.Channel, log.Status);
        
        // TODO:
        // - Kung status ay "Queued", i-trigger ang aktwal na pagpapadala sa background
        // - I-save ang metadata (duration, provider message ID, atbp.)
        
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(NotifyLog log, NotifyLog? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[NOTIFYLOG] NotifyLog {Id} updated", log.Id);
        
        if (originalEntity != null)
        {
            // Kapag nagbago ang status (hal. Queued -> Sent, Queued -> Failed)
            if (originalEntity.Status != log.Status)
            {
                _logger.LogInformation("   → Status changed from '{OldStatus}' → '{NewStatus}'", originalEntity.Status, log.Status);
                
                if (log.Status == "Sent")
                {
                    // TODO: i-record ang successful delivery, i-update ang metrics
                }
                else if (log.Status == "Failed")
                {
                    _logger.LogWarning("   → Failed with error: {ErrorMessage}", log.ErrorMessage);
                    // TODO: i-queue para sa retry o mag-alert sa admin
                }
            }
            
            // Kung nagkaroon ng retry
            if (log.RetryCount > originalEntity.RetryCount)
            {
                _logger.LogInformation("   → Retry count increased to {RetryCount}", log.RetryCount);
                // TODO: i-delay ang susunod na pagsubok
            }
        }
        
        return Task.CompletedTask;
    }

    // Ang NotifyLog ay may Status property, kaya ito ay gagamitin
    public Task OnStatusChangedAsync(NotifyLog log, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogInformation("[NOTIFYLOG] Explicit status change: {Old} → {New} for log {Id}", oldStatus, newStatus, log.Id);
        
        switch (newStatus)
        {
            case "Sent":
                // TODO: i-record ang sent time, i-link sa notification record
                break;
            case "Failed":
                // TODO: mag-queue ng retry (hanggang max attempts)
                break;
            case "Resend":
                // TODO: magpadala ulit gamit ang parehong payload
                break;
        }
        
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(NotifyLog entity, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}