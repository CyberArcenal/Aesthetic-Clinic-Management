using AestheticClinicAPI.Modules.Treatments.Models;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Treatments.StateTransitionService;

public class TreatmentStateTransition : IStateTransitionService<Treatment>
{
    private readonly ILogger<TreatmentStateTransition> _logger;
    private readonly IUserService _userService;
    private readonly INotifyLogService _notifyLogService;

    public TreatmentStateTransition(
        ILogger<TreatmentStateTransition> logger,
        IUserService userService,
        INotifyLogService notifyLogService)
    {
        _logger = logger;
        _userService = userService;
        _notifyLogService = notifyLogService;
    }

    public async Task OnCreatedAsync(Treatment treatment, CancellationToken ct = default)
    {
        _logger.LogInformation("[TREATMENT] New treatment created: '{Name}' (ID: {Id}, Category: {Category}, Price: {Price})",
            treatment.Name, treatment.Id, treatment.Category, treatment.Price);

        // Notify all admins about new treatment
        await NotifyAdmins("New Treatment Added", $@"
A new treatment has been added to the catalog:

**Treatment:** {treatment.Name}
**Category:** {treatment.Category ?? "Uncategorized"}
**Price:** ₱{treatment.Price:N2}
**Duration:** {treatment.DurationMinutes} minutes

Please log into the admin dashboard for details.
");
    }

    public async Task OnUpdatedAsync(Treatment treatment, Treatment? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[TREATMENT] Treatment {Id} ('{Name}') updated", treatment.Id, treatment.Name);

        if (originalEntity == null) return;

        if (originalEntity.Price != treatment.Price)
        {
            _logger.LogInformation("   → Price changed from {Old:C} to {New:C}", originalEntity.Price, treatment.Price);
            await NotifyAdmins("Treatment Price Changed", $@"
The price for **{treatment.Name}** has been updated:

**Old Price:** ₱{originalEntity.Price:N2}
**New Price:** ₱{treatment.Price:N2}

Please review if any promotional materials need updating.
");
        }

        if (originalEntity.DurationMinutes != treatment.DurationMinutes)
        {
            _logger.LogInformation("   → Duration changed from {Old} min to {New} min",
                originalEntity.DurationMinutes, treatment.DurationMinutes);
            // Notify scheduling team (optional – can be added)
        }

        if (originalEntity.Category != treatment.Category)
        {
            _logger.LogInformation("   → Category changed from '{Old}' to '{New}'",
                originalEntity.Category, treatment.Category);
            // Could reorganize catalog – log only
        }
    }

    public async Task OnActiveChangedAsync(Treatment treatment, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        _logger.LogInformation("[TREATMENT] Treatment '{Name}' active changed: {OldActive} → {NewActive}",
            treatment.Name, oldActive, newActive);

        string action = newActive ? "activated" : "deactivated";
        await NotifyAdmins($"Treatment {action}: {treatment.Name}", $@"
Treatment **{treatment.Name}** has been {action} in the catalog.

**Details:**
- Category: {treatment.Category ?? "Uncategorized"}
- Price: ₱{treatment.Price:N2}
- Duration: {treatment.DurationMinutes} minutes

{(newActive ? "It is now available for booking." : "Future bookings will be prevented.")}
");
    }

    // Treatment has IsActive, but we don't have separate Status property. Keep as no‑op.
    public Task OnStatusChangedAsync(Treatment treatment, string oldStatus, string newStatus, CancellationToken ct = default)
        => Task.CompletedTask;

    private async Task NotifyAdmins(string subject, string messageBody)
    {
        // Get all admin users
        var adminUsersResult = await _userService.GetAllAsync();
        if (!adminUsersResult.IsSuccess || adminUsersResult.Data == null)
        {
            _logger.LogWarning("Could not retrieve admin users for treatment notification.");
            return;
        }

        var adminEmails = adminUsersResult.Data
            .Where(u => u.Roles != null && u.Roles.Contains("Admin"))
            .Select(u => u.Email)
            .Where(e => !string.IsNullOrEmpty(e))
            .ToList();

        if (!adminEmails.Any())
        {
            _logger.LogWarning("No admin emails found to send treatment notification.");
            return;
        }

        foreach (var email in adminEmails)
        {
            await _notifyLogService.CreateAsync(new QueueNotificationDto
            {
                Recipient = email,
                Channel = "Email",
                Type = "custom",
                Subject = subject,
                Message = messageBody
            });
        }

        _logger.LogInformation("Sent treatment notification to {Count} admin(s)", adminEmails.Count);
    }
}