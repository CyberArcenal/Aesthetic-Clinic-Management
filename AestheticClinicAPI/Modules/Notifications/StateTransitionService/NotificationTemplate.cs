using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Shared;
using Microsoft.Extensions.Caching.Memory;

namespace AestheticClinicAPI.Modules.Notifications.StateTransitionService;

public class NotificationTemplateStateTransition : IStateTransitionService<NotificationTemplate>
{
    private readonly ILogger<NotificationTemplateStateTransition> _logger;
    private readonly IMemoryCache _cache;

    public NotificationTemplateStateTransition(ILogger<NotificationTemplateStateTransition> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public Task OnCreatedAsync(NotificationTemplate template, CancellationToken ct = default)
    {
        _logger.LogInformation("[TEMPLATE] New template created: {Name}", template.Name);
        _cache.Remove($"template_{template.Name}");
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(NotificationTemplate template, NotificationTemplate? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[TEMPLATE] Template updated: {Name}", template.Name);
        _cache.Remove($"template_{template.Name}");
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(NotificationTemplate template, string oldStatus, string newStatus, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task OnActiveChangedAsync(NotificationTemplate entity, bool oldActive, bool newActive, CancellationToken ct = default)
        => Task.CompletedTask;
}