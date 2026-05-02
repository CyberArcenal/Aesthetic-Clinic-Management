using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.Models;

namespace AestheticClinicAPI.Modules.Authentications.Services
{
    public interface IRefreshTokenService
    {
        Task<ServiceResult<RefreshToken>> CreateTokenAsync(int userId);
        Task<ServiceResult<bool>> RevokeTokenAsync(string token);
        Task<ServiceResult<bool>> RevokeAllForUserAsync(int userId);
        Task<ServiceResult<bool>> CleanExpiredAsync();
    }
}