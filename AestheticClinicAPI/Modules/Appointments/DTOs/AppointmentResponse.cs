namespace AestheticClinicAPI.Modules.Appointments.DTOs
{
    public class AppointmentResponseDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string? ClientName { get; set; }
        public int TreatmentId { get; set; }
        public string? TreatmentName { get; set; }
        public string? AssignedStaff { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}