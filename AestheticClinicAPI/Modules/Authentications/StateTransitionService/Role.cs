using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Authentications.StateTransitionService;

public class RoleStateTransition : IStateTransitionService<Role>
{
    private readonly ILogger<RoleStateTransition> _logger;

    public RoleStateTransition(ILogger<RoleStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(Role role, CancellationToken ct = default)
    {
        _logger.LogInformation("[AUTH ROLE] New role created: '{Name}' (ID: {Id})", role.Name, role.Id);
        // TODO: i-clear ang role cache, i-assign default permissions (kung may permission system)
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Role role, Role? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[AUTH ROLE] Role {Id} ('{Name}') updated", role.Id, role.Name);
        if (originalEntity != null && originalEntity.Name != role.Name)
        {
            _logger.LogInformation("   → Name changed from '{Old}' to '{New}'", originalEntity.Name, role.Name);
            // TODO: i-update ang mga user role references (kung kinakailangan)
        }
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(Role role, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogDebug("[AUTH ROLE] OnStatusChangedAsync called but Role has no Status field.");
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(Role role, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        _logger.LogDebug("[AUTH ROLE] OnActiveChangedAsync called but Role has no IsActive field.");
        return Task.CompletedTask;
    }
}