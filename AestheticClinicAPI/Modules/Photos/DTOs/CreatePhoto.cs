using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Photos.DTOs
{
    public class CreatePhotoDto
    {
        [Required]
        public int ClientId { get; set; }

        public int? AppointmentId { get; set; }

        [Required, MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsBefore { get; set; } = true;

        public long FileSize { get; set; }

        [MaxLength(50)]
        public string? MimeType { get; set; }
    }
}