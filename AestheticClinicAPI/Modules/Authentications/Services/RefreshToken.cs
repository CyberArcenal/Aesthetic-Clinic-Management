using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.Repositories;

namespace AestheticClinicAPI.Modules.Authentications.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepo;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepo)
        {
            _refreshTokenRepo = refreshTokenRepo;
        }

        private string GenerateRandomToken()
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<ServiceResult<RefreshToken>> CreateTokenAsync(int userId)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                Token = GenerateRandomToken(),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            var created = await _refreshTokenRepo.AddAsync(token);
            return ServiceResult<RefreshToken>.Success(created);
        }

        public async Task<ServiceResult<bool>> RevokeTokenAsync(string token)
        {
            var tokenEntity = await _refreshTokenRepo.GetByTokenAsync(token);
            if (tokenEntity == null)
                return ServiceResult<bool>.Failure("Token not found.");
            tokenEntity.IsRevoked = true;
            await _refreshTokenRepo.UpdateAsync(tokenEntity);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> RevokeAllForUserAsync(int userId)
        {
            await _refreshTokenRepo.RevokeAllForUserAsync(userId);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> CleanExpiredAsync()
        {
            await _refreshTokenRepo.CleanExpiredTokensAsync();
            return ServiceResult<bool>.Success(true);
        }
    }
}