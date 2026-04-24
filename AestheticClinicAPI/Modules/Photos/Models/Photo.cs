using AestheticClinicAPI.Modules.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Photos.Models
{
    public class Photo : BaseEntity
    {
        [Required]
        public int ClientId { get; set; }

        public int? AppointmentId { get; set; }  // optional – links to a specific treatment session

        [Required, MaxLength(255)]
        public string FileName { get; set; } = string.Empty;  // original file name

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;  // storage path or URL

        public string? Description { get; set; }

        public bool IsBefore { get; set; } = true;  // true = before treatment, false = after

        public long FileSize { get; set; }  // in bytes

        [MaxLength(50)]
        public string? MimeType { get; set; }  // e.g., "image/jpeg"
    }
}