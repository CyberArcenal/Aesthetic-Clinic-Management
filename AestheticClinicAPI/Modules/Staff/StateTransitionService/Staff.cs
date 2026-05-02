using AestheticClinicAPI.Modules.Staff.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Staff.StateTransitionService;

public class StaffMemberStateTransition : IStateTransitionService<StaffMember>
{
    private readonly ILogger<StaffMemberStateTransition> _logger;

    public StaffMemberStateTransition(ILogger<StaffMemberStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(StaffMember staff, CancellationToken ct = default)
    {
        _logger.LogInformation("[STAFF] New staff member created: {Name} (ID: {Id}, Position: {Position}, Email: {Email})",
            staff.Name, staff.Id, staff.Position, staff.Email);

        // TODO:
        // - Mag-send ng welcome email sa staff
        // - I-create ang user account sa authentication system
        // - I-assign ng default role (Staff)
        // - I-add sa scheduling system

        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(StaffMember staff, StaffMember? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[STAFF] Staff {Id} details updated", staff.Id);

        if (originalEntity != null)
        {
            // Kung nagbago ang position
            if (originalEntity.Position != staff.Position)
            {
                _logger.LogInformation("   → Position changed from '{Old}' to '{New}'",
                    originalEntity.Position, staff.Position);
                // TODO: i-update ang permissions sa system
            }

            // Kung nagbago ang email
            if (originalEntity.Email != staff.Email)
            {
                _logger.LogInformation("   → Email changed from '{Old}' to '{New}'",
                    originalEntity.Email, staff.Email);
                // TODO: i-update ang user account email
            }

            // Kung nagbago ang phone
            if (originalEntity.Phone != staff.Phone)
            {
                _logger.LogInformation("   → Phone changed from '{Old}' to '{New}'",
                    originalEntity.Phone, staff.Phone);
                // TODO: i-update sa notification system
            }
        }

        return Task.CompletedTask;
    }

    // Upang suportahan ang IsActive bilang "status"
    public Task OnStatusChangedAsync(StaffMember staff, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogInformation("[STAFF] Staff {Id} active status changed from '{Old}' → '{New}'",
            staff.Id, oldStatus, newStatus);

        switch (newStatus)
        {
            case "Active":
                // TODO: i-activate ang user account, i-add sa scheduling rotation
                break;
            case "Inactive":
                // TODO: i-deactivate ang user account, i-block ang login, i-remove sa schedule
                break;
        }

        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(StaffMember staff, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        _logger.LogInformation("[STAFF] Staff {Id} active status changed: {OldActive} → {NewActive}",
            staff.Id, oldActive, newActive);

        if (newActive)
        {
            // TODO: i-activate ang user account, i-add sa scheduling rotation
        }
        else
        {
            // TODO: i-deactivate ang user account, i-block ang login, i-remove sa schedule
        }

        return Task.CompletedTask;
    }
}