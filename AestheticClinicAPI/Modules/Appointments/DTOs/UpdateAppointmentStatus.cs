using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Appointments.DTOs
{
    public class UpdateAppointmentStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}