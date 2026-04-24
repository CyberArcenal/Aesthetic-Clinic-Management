using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Authentications.DTOs
{
    public class UserRoleResponseDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}