using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.DTOs;

namespace AestheticClinicAPI.Modules.Authentications.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto);
        Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<ServiceResult<bool>> LogoutAsync(int userId);
        Task<ServiceResult<AuthResponseDto>> GetCurrentUserAsync(int userId);
        Task<ServiceResult<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}