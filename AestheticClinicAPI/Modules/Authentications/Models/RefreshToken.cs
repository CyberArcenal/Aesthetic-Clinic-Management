using AestheticClinicAPI.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Authentications.Models
{
    public class RefreshToken : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Required, MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; } = false;
    }
}