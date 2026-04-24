using AestheticClinicAPI.Modules.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Authentications.Models
{
    public class UserRole : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}