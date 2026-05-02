using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Authentications.StateTransitionService;

public class UserRoleStateTransition : IStateTransitionService<UserRole>
{
    private readonly ILogger<UserRoleStateTransition> _logger;

    public UserRoleStateTransition(ILogger<UserRoleStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(UserRole userRole, CancellationToken ct = default)
    {
        _logger.LogInformation("[AUTH USERROLE] User {UserId} assigned to Role {RoleId}", userRole.UserId, userRole.RoleId);
        // TODO: i-clear ang user permissions cache
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(UserRole userRole, UserRole? originalEntity, CancellationToken ct = default)
    {
        // Karaniwang hindi nag-a-update ang UserRole (i-add or i-remove lang).
        // Pero kung may rason, log lang.
        _logger.LogDebug("[AUTH USERROLE] UserRole {Id} updated", userRole.Id);
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(UserRole userRole, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogDebug("[AUTH USERROLE] OnStatusChangedAsync called but UserRole has no Status field.");
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(UserRole userRole, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        _logger.LogDebug("[AUTH USERROLE] OnActiveChangedAsync called but UserRole has no IsActive field.");
        return Task.CompletedTask;
    }
}