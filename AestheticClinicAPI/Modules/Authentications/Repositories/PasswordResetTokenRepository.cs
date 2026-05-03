using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Authentications.Repositories;

public class PasswordResetTokenRepository : Repository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(AppDbContext context) : base(context) { }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiryDate > DateTime.UtcNow);
    }

    public async Task RevokeAllForUserAsync(int userId)
    {
        var tokens = await _dbSet.Where(t => t.UserId == userId && !t.IsUsed).ToListAsync();
        foreach (var token in tokens)
        {
            token.IsUsed = true;
        }
        if (tokens.Any())
            await _context.SaveChangesAsync();
    }
}