namespace AestheticClinicAPI.Modules.Appointments.DTOs
{
    public class UpdateAppointmentDto
    {
        public int? ClientId { get; set; }
        public int? TreatmentId { get; set; }
        public string? AssignedStaff { get; set; }
        public DateTime? AppointmentDateTime { get; set; }
        public string? Notes { get; set; }
    }
}