using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.Models;

namespace AestheticClinicAPI.Modules.Authentications.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task RevokeAllForUserAsync(int userId);
        Task CleanExpiredTokensAsync();
    }
}