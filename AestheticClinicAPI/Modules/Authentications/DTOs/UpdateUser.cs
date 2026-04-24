namespace AestheticClinicAPI.Modules.Authentications.DTOs
{
    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public bool? IsActive { get; set; }
        public IEnumerable<string>? RolesToAdd { get; set; }
        public IEnumerable<string>? RolesToRemove { get; set; }
    }
}