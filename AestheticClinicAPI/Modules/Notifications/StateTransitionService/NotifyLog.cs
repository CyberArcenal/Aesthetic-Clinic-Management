using System.Text.Json;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Notifications.Channels;
using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Notifications.StateTransitionService;

public class NotifyLogStateTransition : IStateTransitionService<NotifyLog>
{
    private readonly ILogger<NotifyLogStateTransition> _logger;
    private readonly AppDbContext _dbContext;
    private readonly INotificationTemplateService _templateService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IPushService _pushService;

    public NotifyLogStateTransition(
           ILogger<NotifyLogStateTransition> logger,
           AppDbContext dbContext,
           INotificationTemplateService templateService,
           IEmailService emailService,
           ISmsService smsService,
           IPushService pushService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _templateService = templateService;
        _emailService = emailService;
        _smsService = smsService;
        _pushService = pushService;
    }

    public async Task OnCreatedAsync(NotifyLog log, CancellationToken ct = default)
    {
        _logger.LogInformation("[NOTIFYLOG] Processing queued log {Id} for {Recipient}", log.Id, log.RecipientEmail);

        var startTime = DateTime.UtcNow;
        bool success = false;
        string? error = null;
        string renderedSubject = log.Subject ?? "";
        string renderedBody = log.Payload ?? "";

        try
        {
            // 1. Kung may template (hindi "custom"), i-render ito
            if (!string.IsNullOrEmpty(log.Type) && log.Type != "custom")
            {
                var templateResult = await _templateService.GetByNameAsync(log.Type);
                if (!templateResult.IsSuccess || templateResult.Data == null)
                    throw new Exception($"Template '{log.Type}' not found.");

                var template = templateResult.Data;
                renderedSubject = template.Subject;
                renderedBody = template.Content;

                var metadata = log.Metadata != null
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(log.Metadata)
                    : null;

                if (metadata != null)
                {
                    foreach (var kv in metadata)
                    {
                        renderedSubject = renderedSubject.Replace($"{{{{ {kv.Key} }}}}", kv.Value);
                        renderedBody = renderedBody.Replace($"{{{{ {kv.Key} }}}}", kv.Value);
                    }
                }
            }

            // 2. Ipadala sa tamang channel
            switch (log.Channel.ToLower())
            {
                case "email":
                    success = await _emailService.SendSimpleEmailAsync(log.RecipientEmail, renderedSubject, renderedBody);
                    break;
                case "sms":
                    success = await _smsService.SendSmsAsync(log.RecipientEmail, renderedBody);
                    break;
                case "push":
                    success = await _pushService.SendPushAsync(log.RecipientEmail, renderedSubject, renderedBody);
                    break;
                default:
                    error = $"Unknown channel '{log.Channel}'";
                    success = false;
                    break;
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
            success = false;
            _logger.LogError(ex, "Notification send failed for log {Id}", log.Id);
        }

        var durationMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

        // 3. I‑update ang entity (directly attached)
        log.Status = success ? "Sent" : "Failed";
        log.DurationMs = durationMs;
        if (success)
        {
            log.SentAt = DateTime.UtcNow;
            log.Subject = renderedSubject;
            log.Payload = renderedBody;
        }
        else
        {
            log.ErrorMessage = error;
            log.LastErrorAt = DateTime.UtcNow;
        }

        // Mark as modified (EF will detect changes automatically)
        _dbContext.Entry(log).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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
        => Task.CompletedTask;
}