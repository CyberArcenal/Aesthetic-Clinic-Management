using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.Repositories;
using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Modules.Clients.Repositories;
using AestheticClinicAPI.Modules.Staff.Models;
using AestheticClinicAPI.Modules.Staff.Repositories;
using AestheticClinicAPI.Shared;
using Microsoft.EntityFrameworkCore;

namespace AestheticClinicAPI.Modules.Authentications.StateTransitionService;

public class UserStateTransition : IStateTransitionService<User>
{
    private readonly ILogger<UserStateTransition> _logger;
    private readonly AppDbContext _dbContext;
    private readonly IClientRepository _clientRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo; // new

    public UserStateTransition(
        ILogger<UserStateTransition> logger,
        AppDbContext dbContext,
        IClientRepository clientRepo,
        IStaffRepository staffRepo,
        IRefreshTokenRepository refreshTokenRepo
    )
    {
        _logger = logger;
        _dbContext = dbContext;
        _clientRepo = clientRepo;
        _staffRepo = staffRepo;
        _refreshTokenRepo = refreshTokenRepo;
    }

    public async Task OnCreatedAsync(User user, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[AUTH USER] New user created: Username '{Username}', Email '{Email}', FullName '{FullName}'",
            user.Username,
            user.Email,
            user.FullName
        );

        // Get roles assigned to this user from the change tracker (since they are added in the same transaction)
        var userRoles = _dbContext
            .ChangeTracker.Entries<UserRole>()
            .Where(e => e.State == EntityState.Added && e.Entity.UserId == user.Id)
            .Select(e => e.Entity)
            .ToList();

        if (!userRoles.Any())
        {
            _logger.LogWarning(
                "No UserRole entries found in change tracker for new user {UserId}. Skipping related entity creation.",
                user.Id
            );
            return;
        }

        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = _dbContext
            .ChangeTracker.Entries<Role>()
            .Where(e => e.State == EntityState.Unchanged && roleIds.Contains(e.Entity.Id))
            .Select(e => e.Entity.Name)
            .ToList();

        if (!roles.Any())
        {
            _logger.LogWarning(
                "Could not determine role names for new user {UserId}. Skipping related entity creation.",
                user.Id
            );
            return;
        }

        if (roles.Contains("Client"))
        {
            var client = new Client
            {
                FirstName = user.FullName?.Split(' ')[0] ?? user.Username,
                LastName = user.FullName?.Split(' ').LastOrDefault() ?? "",
                Email = user.Email,
                PhoneNumber = null,
                DateOfBirth = null,
                SkinHistory = null,
                Allergies = null,
                CreatedAt = DateTime.UtcNow,
            };
            await _clientRepo.AddAsync(client); // No cancellation token here
            _logger.LogInformation("   → Auto‑created Client record for user {UserId}", user.Id);
        }

        if (roles.Contains("Staff"))
        {
            var staff = new StaffMember
            {
                Name = user.FullName ?? user.Username,
                Email = user.Email,
                Phone = null,
                Position = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            await _staffRepo.AddAsync(staff); // No cancellation token here
            _logger.LogInformation(
                "   → Auto‑created StaffMember record for user {UserId}",
                user.Id
            );
        }
    }

    public Task OnUpdatedAsync(User user, User? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[AUTH USER] User {Id} ('{Username}') updated",
            user.Id,
            user.Username
        );

        if (originalEntity != null)
        {
            if (originalEntity.Email != user.Email)
            {
                _logger.LogInformation(
                    "   → Email changed from '{Old}' to '{New}'",
                    originalEntity.Email,
                    user.Email
                );
                // TODO: magpadala ng verification sa bagong email
            }
            if (originalEntity.Username != user.Username)
            {
                _logger.LogInformation(
                    "   → Username changed from '{Old}' to '{New}'",
                    originalEntity.Username,
                    user.Username
                );
                // TODO: i-update ang mga associated records (e.g., notifications)
            }
            if (originalEntity.FullName != user.FullName)
            {
                _logger.LogInformation(
                    "   → FullName changed from '{Old}' to '{New}'",
                    originalEntity.FullName,
                    user.FullName
                );
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
    public Task OnStatusChangedAsync(
        User user,
        string oldStatus,
        string newStatus,
        CancellationToken ct = default
    )
    {
        _logger.LogDebug(
            "[AUTH USER] OnStatusChangedAsync called but User has no Status field. Ignoring."
        );
        return Task.CompletedTask;
    }

    // May IsActive si User para sa active/inactive status
    public async Task OnActiveChangedAsync(
        User user,
        bool oldActive,
        bool newActive,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation(
            "[AUTH USER] User '{Username}' active status changed: {OldActive} → {NewActive}",
            user.Username,
            oldActive,
            newActive
        );

        if (!newActive) // deactivated
        {
            // Revoke all refresh tokens to force logout
            await _refreshTokenRepo.RevokeAllForUserAsync(user.Id);
            _logger.LogInformation("   → Revoked all refresh tokens for user {UserId}", user.Id);
        }
        else
        {
            // Reactivated – optionally send welcome back email or log
            _logger.LogInformation("   → User {UserId} reactivated", user.Id);
        }
    }
}
