using AestheticClinicAPI.Modules.Appointments.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Appointments.StateTransitionService;

public class AppointmentStateTransition : IStateTransitionService<Appointment>
{
    private readonly ILogger<AppointmentStateTransition> _logger;

    public AppointmentStateTransition(ILogger<AppointmentStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(Appointment appointment, CancellationToken ct = default)
    {
        _logger.LogInformation("[APPOINTMENT] OnCreatedAsync called for Appointment ID: {Id}, ClientId: {ClientId}",
            appointment.Id, appointment.ClientId);
        // TODO: magpadala ng confirmation email/SMS, mag-create ng invoice, atbp.
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Appointment appointment, Appointment? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[APPOINTMENT] OnUpdatedAsync called for Appointment ID: {Id}", appointment.Id);
        if (originalEntity != null)
        {
            // Halimbawa: kung nagbago ang appointment date/time
            if (originalEntity.AppointmentDateTime != appointment.AppointmentDateTime)
            {
                _logger.LogInformation("   → Appointment datetime changed from {Old} to {New}",
                    originalEntity.AppointmentDateTime, appointment.AppointmentDateTime);
                // TODO: magpadala ng reschedule notification
            }
        }
        return Task.CompletedTask;
    }

    public virtual Task OnStatusChangedAsync(Appointment appointment, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogInformation("[APPOINTMENT] OnStatusChangedAsync: Appointment {Id} status from '{Old}' → '{New}'",
            appointment.Id, oldStatus, newStatus);

        // Placeholder para sa iba't ibang status transitions
        switch (newStatus)
        {
            case "Confirmed":
                // TODO: i-send ang confirmation
                break;
            case "Cancelled":
                // TODO: mag-email sa client, free up staff schedule
                break;
            case "Completed":
                // TODO: i-generate ang invoice, magpadala ng feedback request
                break;
            case "NoShow":
                // TODO: mag-charge ng no-show fee kung applicable
                break;
        }
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(Appointment entity, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}