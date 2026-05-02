using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Appointments.Models;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Modules.Appointments.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetByClientAsync(int clientId);
        Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Appointment>> GetByStatusAsync(string status);
        Task<bool> IsTimeSlotAvailableAsync(int staffId, DateTime startTime, int durationMinutes, int? excludeAppointmentId = null);
        Task<PaginatedResult<Appointment>> GetPaginatedWithDetailsAsync(int page, int pageSize, Expression<Func<Appointment, bool>>? filter = null);
    }
}