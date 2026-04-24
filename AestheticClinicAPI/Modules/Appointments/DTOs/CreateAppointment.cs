using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Appointments.DTOs
{
    public class CreateAppointmentDto
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        public int TreatmentId { get; set; }

        public string? AssignedStaff { get; set; }

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        public string? Notes { get; set; }
    }
}