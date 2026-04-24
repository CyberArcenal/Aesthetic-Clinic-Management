using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Photos.Models;

namespace AestheticClinicAPI.Modules.Photos.Repositories
{
    public interface IPhotoRepository : IRepository<Photo>
    {
        Task<IEnumerable<Photo>> GetByClientAsync(int clientId);
        Task<IEnumerable<Photo>> GetByAppointmentAsync(int appointmentId);
        Task<IEnumerable<Photo>> GetBeforePhotosAsync(int clientId);
        Task<IEnumerable<Photo>> GetAfterPhotosAsync(int clientId);
    }
}