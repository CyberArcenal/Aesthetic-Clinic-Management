using AestheticClinicAPI.Modules.Appointments.Repositories;
using Microsoft.Extensions.Logging;

namespace AestheticClinicAPI.Modules.Appointments.Subscribers
{
    public class AppointmentSubscriber
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly ILogger<AppointmentSubscriber> _logger;

        public AppointmentSubscriber(IAppointmentRepository appointmentRepo, ILogger<AppointmentSubscriber> logger)
        {
            _appointmentRepo = appointmentRepo;
            _logger = logger;
        }

        public async Task OnConfirmed(int appointmentId)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
            if (appointment == null) return;
            _logger.LogInformation("Appointment {AppointmentId} confirmed for client {ClientId}", appointmentId, appointment.ClientId);
            // TODO: Send confirmation email/SMS, update calendar, etc.
        }

        public async Task OnCompleted(int appointmentId)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
            if (appointment == null) return;
            _logger.LogInformation("Appointment {AppointmentId} completed for client {ClientId}", appointmentId, appointment.ClientId);
            // TODO: Trigger billing, send follow-up survey, etc.
        }

        public async Task OnCancelled(int appointmentId)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
            if (appointment == null) return;
            _logger.LogInformation("Appointment {AppointmentId} cancelled for client {ClientId}", appointmentId, appointment.ClientId);
            // TODO: Notify staff, release time slot, etc.
        }

        public async Task OnNoShow(int appointmentId)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
            if (appointment == null) return;
            _logger.LogInformation("Appointment {AppointmentId} marked as no-show for client {ClientId}", appointmentId, appointment.ClientId);
            // TODO: Apply no-show fee, notify staff, etc.
        }
    }
}