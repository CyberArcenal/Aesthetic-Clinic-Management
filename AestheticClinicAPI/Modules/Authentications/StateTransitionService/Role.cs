using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Shared;
using Microsoft.Extensions.Caching.Memory;

namespace AestheticClinicAPI.Modules.Authentications.StateTransitionService;

public class RoleStateTransition : IStateTransitionService<Role>
{
    private readonly ILogger<RoleStateTransition> _logger;
    private readonly IMemoryCache _cache;

    public RoleStateTransition(ILogger<RoleStateTransition> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public Task OnCreatedAsync(Role role, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[AUTH ROLE] New role created: '{Name}' (ID: {Id})",
            role.Name,
            role.Id
        );
        _cache.Remove("all_roles"); // clear any cached role lists
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Role role, Role? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[AUTH ROLE] Role {Id} ('{Name}') updated", role.Id, role.Name);
        if (originalEntity != null && originalEntity.Name != role.Name)
        {
            _logger.LogInformation(
                "   → Name changed from '{Old}' to '{New}'",
                originalEntity.Name,
                role.Name
            );
        }
        _cache.Remove("all_roles");
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(
        Role role,
        string oldStatus,
        string newStatus,
        CancellationToken ct = default
    ) => Task.CompletedTask;

    public Task OnActiveChangedAsync(
        Role role,
        bool oldActive,
        bool newActive,
        CancellationToken ct = default
    ) => Task.CompletedTask;
}
