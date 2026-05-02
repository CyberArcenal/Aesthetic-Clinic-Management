using AestheticClinicAPI.Shared;
using System.ComponentModel.DataAnnotations;
using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Modules.Treatments.Models;

namespace AestheticClinicAPI.Modules.Appointments.Models
{
    public class Appointment : BaseEntity
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        public int TreatmentId { get; set; }

        public int? StaffId { get; set; }  // optional, assigned staff

        [MaxLength(100)]
        public string? AssignedStaff { get; set; }  // name of staff member

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        public int DurationMinutes { get; set; }  // copied from treatment

        public string? Notes { get; set; }

        [Required]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, Completed, Cancelled, NoShow

        // Navigation properties
        public virtual Client? Client { get; set; }
        public virtual Treatment? Treatment { get; set; }
    }
}