using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Notifications.StateTransitionService;

public class NotificationStateTransition : IStateTransitionService<Notification>
{
    private readonly ILogger<NotificationStateTransition> _logger;

    public NotificationStateTransition(ILogger<NotificationStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(Notification notification, CancellationToken ct = default)
    {
        _logger.LogInformation("[NOTIFICATION] New notification created for RecipientId: {RecipientId}, Type: {Type}, Channel: {Channel}",
            notification.RecipientId, notification.Type, notification.Channel);
        
        // TODO:
        // - I-queue ang notification para i-send sa external provider (email/SMS/push)
        // - Mag-create ng NotifyLog entry para sa audit trail
        // - Kung InApp, i-store lang sa database
        
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Notification notification, Notification? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[NOTIFICATION] Notification {Id} updated", notification.Id);
        
        if (originalEntity != null)
        {
            // Kung nagbago ang IsRead status (binasa na ng user)
            if (originalEntity.IsRead != notification.IsRead && notification.IsRead)
            {
                _logger.LogInformation("   → Notification marked as read at {ReadAt}", notification.ReadAt);
                // TODO: mag-trigger ng read receipt o analytics event
            }
        }
        
        return Task.CompletedTask;
    }

    // Walang explicit Status property ang Notification (but may IsRead na parang status)
    // Pwedeng i-monitor ang IsRead gamit ito kung gusto, pero iiwan muna natin bilang placeholder
    public Task OnStatusChangedAsync(Notification notification, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogDebug("[NOTIFICATION] OnStatusChangedAsync called but Notification has no Status field. Ignoring.");
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(Notification entity, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}