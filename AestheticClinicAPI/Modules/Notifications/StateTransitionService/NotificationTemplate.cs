using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Notifications.StateTransitionService;

public class NotificationTemplateStateTransition : IStateTransitionService<NotificationTemplate>
{
    private readonly ILogger<NotificationTemplateStateTransition> _logger;

    public NotificationTemplateStateTransition(ILogger<NotificationTemplateStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(NotificationTemplate template, CancellationToken ct = default)
    {
        _logger.LogInformation("[TEMPLATE] New notification template created: '{Name}' (ID: {Id})", template.Name, template.Id);
        
        // TODO:
        // - I-validate ang syntax ng template (placeholder consistency)
        // - I-store sa cache para sa mabilis na rendering
        
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(NotificationTemplate template, NotificationTemplate? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[TEMPLATE] Template '{Name}' (ID: {Id}) updated", template.Name, template.Id);
        
        if (originalEntity != null)
        {
            // Kung nabago ang content
            if (originalEntity.Content != template.Content)
            {
                _logger.LogInformation("   → Template content changed");
                // TODO: re-validate at i-clear ang cache para sa template na ito
            }
        }
        
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(NotificationTemplate template, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogDebug("[TEMPLATE] OnStatusChangedAsync called but Template has no Status field. Ignoring.");
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(NotificationTemplate entity, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}