using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Authentications.Models;

namespace AestheticClinicAPI.Modules.Authentications.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context) { }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);
        }

        public async Task RevokeAllForUserAsync(int userId)
        {
            var tokens = await _dbSet.Where(rt => rt.UserId == userId && !rt.IsRevoked).ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                await UpdateAsync(token);
            }
        }

        public async Task CleanExpiredTokensAsync()
        {
            var expiredTokens = await _dbSet.Where(rt => rt.ExpiryDate < DateTime.UtcNow).ToListAsync();
            foreach (var token in expiredTokens)
            {
                await DeleteAsync(token); // soft delete
            }
        }
    }
}