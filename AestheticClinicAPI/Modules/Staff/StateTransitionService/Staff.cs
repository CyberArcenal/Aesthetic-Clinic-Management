using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Repositories;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Staff.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Staff.StateTransitionService;

public class StaffMemberStateTransition : IStateTransitionService<StaffMember>
{
    private readonly ILogger<StaffMemberStateTransition> _logger;
    private readonly IUserService _userService;
    private readonly IUserRoleService _userRoleService;
    private readonly INotifyLogService _notifyLogService;
    private readonly IRoleRepository _roleRepo;
    private readonly IConfiguration _configuration;

    public StaffMemberStateTransition(
        ILogger<StaffMemberStateTransition> logger,
        IUserService userService,
        IUserRoleService userRoleService,
        INotifyLogService notifyLogService,
        IRoleRepository roleRepo,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _userService = userService;
        _userRoleService = userRoleService;
        _notifyLogService = notifyLogService;
        _roleRepo = roleRepo;
        _configuration = configuration;
    }

    public async Task OnCreatedAsync(StaffMember staff, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[STAFF] New staff member created: {Name} (ID: {Id}, Position: {Position}, Email: {Email})",
            staff.Name,
            staff.Id,
            staff.Position,
            staff.Email
        );

        if (string.IsNullOrEmpty(staff.Email))
        {
            _logger.LogWarning("Staff {Id} has no email – cannot create user account.", staff.Id);
            return;
        }

        var frontendBaseUrl = _configuration["Frontend:BaseUrl"];

        // Find existing user by email
        var existingUserResult = await _userService.GetPaginatedAsync(1, 1, staff.Email);
        UserResponseDto? existingUser = null;
        if (existingUserResult.IsSuccess && existingUserResult.Data?.Items?.Any() == true)
        {
            existingUser = existingUserResult.Data.Items.First();
        }

        if (existingUser == null)
        {
            // Generate a random temporary password (strong, but never revealed to user)
            var randomPassword = GenerateRandomPassword();
            var createDto = new CreateUserDto
            {
                Username = staff.Email.Split('@')[0],
                Email = staff.Email,
                Password = randomPassword,
                FullName = staff.Name,
                IsActive = staff.IsActive,
                Roles = new[] { "Staff" },
            };
            var createResult = await _userService.CreateAsync(createDto);
            if (!createResult.IsSuccess || createResult.Data == null)
            {
                _logger.LogError(
                    "Failed to create user account for staff {Id}: {Error}",
                    staff.Id,
                    createResult.ErrorMessage
                );
                return;
            }
            int userId = createResult.Data.Id;
            _logger.LogInformation(
                "   → Created user account for staff {Id} with username {Username}",
                staff.Id,
                createResult.Data.Username
            );

            // Generate password reset token (reset link)
            var resetTokenResult = await _userService.GeneratePasswordResetTokenAsync(userId);
            if (!resetTokenResult.IsSuccess)
            {
                _logger.LogError(
                    "Failed to generate password reset token for user {UserId}",
                    userId
                );
                return;
            }
            var resetLink =
                $"{frontendBaseUrl}/set-password?token={resetTokenResult.Data}&email={Uri.EscapeDataString(staff.Email)}";

            // Send welcome email with reset link (no plain password)
            var metadata = new Dictionary<string, string>
            {
                { "StaffName", staff.Name },
                { "Email", staff.Email },
                { "Position", staff.Position ?? "Staff" },
                { "ClinicName", "Aesthetic Wellness Clinic" },
                { "PortalUrl", $"{frontendBaseUrl}/staff-login" },
                { "ResetLink", resetLink },
            };
            await _notifyLogService.CreateAsync(
                new QueueNotificationDto
                {
                    Recipient = staff.Email,
                    Channel = "Email",
                    Type = "StaffWelcomeEmail", // template should contain {{ ResetLink }}
                    Metadata = metadata,
                }
            );
        }
        else
        {
            // Existing user – ensure they have Staff role
            int userId = existingUser.Id;
            var staffRole = await _roleRepo.GetByNameAsync("Staff");
            if (staffRole == null)
            {
                _logger.LogWarning(
                    "Role 'Staff' not found – cannot assign role to user {UserId}",
                    userId
                );
                return;
            }

            var assignDto = new AssignRoleDto { UserId = userId, RoleId = staffRole.Id };
            var assignResult = await _userRoleService.AssignRoleAsync(assignDto);
            if (!assignResult.IsSuccess)
            {
                _logger.LogWarning(
                    "   → Could not assign Staff role to existing user {UserId}: {Error}",
                    userId,
                    assignResult.ErrorMessage
                );
            }
            else
            {
                _logger.LogInformation(
                    "   → Assigned Staff role to existing user {UserId}",
                    userId
                );
            }

            // Optional: send notification that they have been added as staff (no password, no reset link)
            var metadata = new Dictionary<string, string>
            {
                { "StaffName", staff.Name },
                { "Email", staff.Email },
                { "Position", staff.Position ?? "Staff" },
                { "ClinicName", "Aesthetic Wellness Clinic" },
                { "PortalUrl", $"{frontendBaseUrl}/staff-login" },
            };
            await _notifyLogService.CreateAsync(
                new QueueNotificationDto
                {
                    Recipient = staff.Email,
                    Channel = "Email",
                    Type = "StaffWelcomeExisting", // separate template or generic
                    Metadata = metadata,
                }
            );
        }
    }

    public async Task OnUpdatedAsync(
        StaffMember staff,
        StaffMember? originalEntity,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("[STAFF] Staff {Id} details updated", staff.Id);
        if (originalEntity == null)
            return;

        if (originalEntity.Email != staff.Email && !string.IsNullOrEmpty(staff.Email))
        {
            _logger.LogInformation(
                "   → Email changed from '{Old}' to '{New}'",
                originalEntity.Email,
                staff.Email
            );
            var user = await FindUserByEmail(originalEntity.Email);
            if (user != null)
            {
                var updateDto = new UpdateUserDto { Email = staff.Email };
                await _userService.UpdateAsync(user.Id, updateDto);
                _logger.LogInformation(
                    "   → Updated user account email to {NewEmail}",
                    staff.Email
                );
            }
        }

        if (originalEntity.Name != staff.Name)
        {
            _logger.LogInformation(
                "   → Name changed from '{Old}' to '{New}'",
                originalEntity.Name,
                staff.Name
            );
            var user = await FindUserByEmail(staff.Email);
            if (user != null)
            {
                var updateDto = new UpdateUserDto { FullName = staff.Name };
                await _userService.UpdateAsync(user.Id, updateDto);
            }
        }

        if (originalEntity.Position != staff.Position)
        {
            _logger.LogInformation(
                "   → Position changed from '{Old}' to '{New}'",
                originalEntity.Position,
                staff.Position
            );
        }
    }

    public async Task OnActiveChangedAsync(
        StaffMember staff,
        bool oldActive,
        bool newActive,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation(
            "[STAFF] Staff {Id} active status changed: {OldActive} → {NewActive}",
            staff.Id,
            oldActive,
            newActive
        );

        if (string.IsNullOrEmpty(staff.Email))
            return;
        var user = await FindUserByEmail(staff.Email);
        if (user != null)
        {
            await _userService.ActivateAsync(user.Id, newActive);
            _logger.LogInformation(
                "   → User account {UserId} {Action}",
                user.Id,
                newActive ? "activated" : "deactivated"
            );
        }
    }

    public Task OnStatusChangedAsync(
        StaffMember staff,
        string oldStatus,
        string newStatus,
        CancellationToken ct = default
    ) => Task.CompletedTask;

    private async Task<UserResponseDto?> FindUserByEmail(string email)
    {
        var result = await _userService.GetPaginatedAsync(1, 1, email);
        return result.IsSuccess && result.Data?.Items?.Any() == true
            ? result.Data.Items.First()
            : null;
    }

    private static string GenerateRandomPassword(int length = 12)
    {
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string allChars = upper + lower + digits + special;

        var random = new Random();
        var password = new char[length];

        password[0] = upper[random.Next(upper.Length)];
        password[1] = lower[random.Next(lower.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        for (int i = 4; i < length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        return new string(password.OrderBy(x => random.Next()).ToArray());
    }
}
