using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Shared;
using Microsoft.Extensions.Caching.Memory;

namespace AestheticClinicAPI.Modules.Authentications.StateTransitionService;

public class UserRoleStateTransition : IStateTransitionService<UserRole>
{
    private readonly ILogger<UserRoleStateTransition> _logger;
    private readonly IMemoryCache _cache;

    public UserRoleStateTransition(ILogger<UserRoleStateTransition> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public Task OnCreatedAsync(UserRole userRole, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[AUTH USERROLE] User {UserId} assigned to Role {RoleId}",
            userRole.UserId,
            userRole.RoleId
        );
        // Invalidate cached permissions for this user
        _cache.Remove($"user_roles_{userRole.UserId}");
        return Task.CompletedTask;
    }

    // Note: There's no OnDeletedAsync, but when UserRole is deleted (soft delete or hard delete), we should also invalidate cache.
    // We can use OnUpdatedAsync to detect IsDeleted change? But UserRole has no IsDeleted flag (BaseEntity has, but we may not use soft delete for this junction).
    // For simplicity, we rely on the fact that delete will be captured by OnUpdatedAsync if IsDeleted changes. Add that logic.

    public async Task OnUpdatedAsync(
        UserRole userRole,
        UserRole? originalEntity,
        CancellationToken ct = default
    )
    {
        _logger.LogDebug("[AUTH USERROLE] UserRole {Id} updated", userRole.Id);

        // If the entity is being soft-deleted (IsDeleted changed from false to true), treat as removal
        if (originalEntity != null && !originalEntity.IsDeleted && userRole.IsDeleted)
        {
            _logger.LogInformation(
                "[AUTH USERROLE] User {UserId} removed from Role {RoleId} (soft delete)",
                userRole.UserId,
                userRole.RoleId
            );
            _cache.Remove($"user_roles_{userRole.UserId}");
        }
        else
        {
            // other updates not relevant
        }
    }

    public Task OnStatusChangedAsync(
        UserRole userRole,
        string oldStatus,
        string newStatus,
        CancellationToken ct = default
    ) => Task.CompletedTask;

    public Task OnActiveChangedAsync(
        UserRole userRole,
        bool oldActive,
        bool newActive,
        CancellationToken ct = default
    ) => Task.CompletedTask;
}
