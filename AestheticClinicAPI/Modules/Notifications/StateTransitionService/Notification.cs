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
        _logger.LogInformation("[NOTIFICATION] New in-app notification for user {RecipientId}: {Title}", 
            notification.RecipientId, notification.Title);
        // In-app notifications are already saved – could trigger SignalR here.
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Notification notification, Notification? originalEntity, CancellationToken ct = default)
    {
        if (originalEntity != null && !originalEntity.IsRead && notification.IsRead)
        {
            _logger.LogInformation("[NOTIFICATION] Notification {Id} marked as read by user {RecipientId}", 
                notification.Id, notification.RecipientId);
        }
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(Notification notification, string oldStatus, string newStatus, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task OnActiveChangedAsync(Notification entity, bool oldActive, bool newActive, CancellationToken ct = default)
        => Task.CompletedTask;
}