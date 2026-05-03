using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.Models;

namespace AestheticClinicAPI.Modules.Authentications.Repositories;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task RevokeAllForUserAsync(int userId);
}