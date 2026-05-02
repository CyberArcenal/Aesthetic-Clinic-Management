using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Appointments.Models;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Modules.Appointments.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Appointment>> GetByClientAsync(int clientId)
        {
            return await _dbSet
                .Where(a => a.ClientId == clientId && !a.IsDeleted)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _dbSet
                .Where(a => a.AppointmentDateTime >= start && a.AppointmentDateTime <= end && !a.IsDeleted)
                .OrderBy(a => a.AppointmentDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(a => a.Status == status && !a.IsDeleted)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int staffId, DateTime startTime, int durationMinutes, int? excludeAppointmentId = null)
        {
            var endTime = startTime.AddMinutes(durationMinutes);
            var query = _dbSet.Where(a => a.StaffId == staffId
                && !a.IsDeleted
                && a.Status != "Cancelled"
                && a.Status != "NoShow"
                && a.AppointmentDateTime < endTime
                && a.AppointmentDateTime.AddMinutes(a.DurationMinutes) > startTime);
            if (excludeAppointmentId.HasValue)
                query = query.Where(a => a.Id != excludeAppointmentId.Value);
            return !await query.AnyAsync();
        }


        public async Task<PaginatedResult<Appointment>> GetPaginatedWithDetailsAsync(int page, int pageSize, Expression<Func<Appointment, bool>>? filter = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var query = _dbSet.Where(a => !a.IsDeleted);
            if (filter != null)
                query = query.Where(filter);
            var totalCount = await query.CountAsync();
            var items = await query
                .Include(a => a.Client)
                .Include(a => a.Treatment)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PaginatedResult<Appointment>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}