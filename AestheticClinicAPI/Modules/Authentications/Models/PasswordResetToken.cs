using AestheticClinicAPI.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Authentications.Models;

public class PasswordResetToken : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    [Required, MaxLength(255)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }

    public bool IsUsed { get; set; } = false;
}