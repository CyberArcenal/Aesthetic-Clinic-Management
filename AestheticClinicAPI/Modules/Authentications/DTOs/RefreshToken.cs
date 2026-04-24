using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Authentications.DTOs
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}