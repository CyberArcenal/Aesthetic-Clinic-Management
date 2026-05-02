using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Authentications.StateTransitionService;

public class UserStateTransition : IStateTransitionService<User>
{
    private readonly ILogger<UserStateTransition> _logger;

    public UserStateTransition(ILogger<UserStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(User user, CancellationToken ct = default)
    {
        _logger.LogInformation("[AUTH USER] New user created: Username '{Username}', Email '{Email}', FullName '{FullName}'",
            user.Username, user.Email, user.FullName);
        
        // TODO:
        // - Magpadala ng welcome email
        // - Mag-assign ng default role (e.g., "Client" or "Staff")
        // - I-log ang creation sa audit trail
        // - Mag-create ng default notification preferences
        
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(User user, User? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[AUTH USER] User {Id} ('{Username}') updated", user.Id, user.Username);
        
        if (originalEntity != null)
        {
            if (originalEntity.Email != user.Email)
            {
                _logger.LogInformation("   → Email changed from '{Old}' to '{New}'", originalEntity.Email, user.Email);
                // TODO: magpadala ng verification sa bagong email
            }
            if (originalEntity.Username != user.Username)
            {
                _logger.LogInformation("   → Username changed from '{Old}' to '{New}'", originalEntity.Username, user.Username);
                // TODO: i-update ang mga associated records (e.g., notifications)
            }
            if (originalEntity.FullName != user.FullName)
            {
                _logger.LogInformation("   → FullName changed from '{Old}' to '{New}'", originalEntity.FullName, user.FullName);
            }
            if (originalEntity.LastLoginAt != user.LastLoginAt && user.LastLoginAt.HasValue)
            {
                _logger.LogInformation("   → Last login at {LastLogin}", user.LastLoginAt);
                // TODO: i-record ang login history
            }
        }
        
        return Task.CompletedTask;
    }

    // Walang Status property si User, kaya placeholder lamang
    public Task OnStatusChangedAsync(User user, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogDebug("[AUTH USER] OnStatusChangedAsync called but User has no Status field. Ignoring.");
        return Task.CompletedTask;
    }

    // May IsActive si User para sa active/inactive status
    public Task OnActiveChangedAsync(User user, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        _logger.LogInformation("[AUTH USER] User '{Username}' active status changed: {OldActive} → {NewActive}",
            user.Username, oldActive, newActive);
        
        if (newActive)
        {
            // TODO: i-enable ang login, i-restore ang access
        }
        else
        {
            // TODO: i-force logout, i-revoke lahat ng refresh tokens
        }
        
        return Task.CompletedTask;
    }
}