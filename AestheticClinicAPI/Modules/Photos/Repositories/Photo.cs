using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Photos.Models;

namespace AestheticClinicAPI.Modules.Photos.Repositories
{
    public class PhotoRepository : Repository<Photo>, IPhotoRepository
    {
        public PhotoRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Photo>> GetByClientAsync(int clientId)
        {
            return await _dbSet
                .Where(p => p.ClientId == clientId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Photo>> GetByAppointmentAsync(int appointmentId)
        {
            return await _dbSet
                .Where(p => p.AppointmentId == appointmentId && !p.IsDeleted)
                .OrderBy(p => p.IsBefore ? 0 : 1) // before first, then after
                .ToListAsync();
        }

        public async Task<IEnumerable<Photo>> GetBeforePhotosAsync(int clientId)
        {
            return await _dbSet
                .Where(p => p.ClientId == clientId && p.IsBefore == true && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Photo>> GetAfterPhotosAsync(int clientId)
        {
            return await _dbSet
                .Where(p => p.ClientId == clientId && p.IsBefore == false && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}