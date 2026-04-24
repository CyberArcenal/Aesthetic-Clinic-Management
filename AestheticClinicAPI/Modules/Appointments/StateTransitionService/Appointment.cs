using AestheticClinicAPI.Modules.Appointments.Constants;
using AestheticClinicAPI.Modules.Appointments.Subscribers;

namespace AestheticClinicAPI.Modules.Appointments.StateTransitionService
{
    public class AppointmentStateTransition
    {
        private readonly AppointmentSubscriber _subscriber;

        public AppointmentStateTransition(AppointmentSubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public async Task HandleStatusChange(int appointmentId, string oldStatus, string newStatus)
        {
            // Only trigger on certain status changes
            if (newStatus == AppointmentStatus.Confirmed && oldStatus != AppointmentStatus.Confirmed)
            {
                await _subscriber.OnConfirmed(appointmentId);
            }
            else if (newStatus == AppointmentStatus.Completed && oldStatus != AppointmentStatus.Completed)
            {
                await _subscriber.OnCompleted(appointmentId);
            }
            else if (newStatus == AppointmentStatus.Cancelled && oldStatus != AppointmentStatus.Cancelled)
            {
                await _subscriber.OnCancelled(appointmentId);
            }
            else if (newStatus == AppointmentStatus.NoShow && oldStatus != AppointmentStatus.NoShow)
            {
                await _subscriber.OnNoShow(appointmentId);
            }
        }
    }
}