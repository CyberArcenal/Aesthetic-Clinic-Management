using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Photos.DTOs;

namespace AestheticClinicAPI.Modules.Photos.Services
{
    public interface IPhotoService
    {
        Task<ServiceResult<IEnumerable<PhotoResponseDto>>> GetByClientAsync(int clientId);
        
        Task<ServiceResult<IEnumerable<PhotoResponseDto>>> GetByAppointmentAsync(int appointmentId);
        Task<ServiceResult<IEnumerable<PhotoResponseDto>>> GetBeforePhotosAsync(int clientId);
        Task<ServiceResult<IEnumerable<PhotoResponseDto>>> GetAfterPhotosAsync(int clientId);
        Task<ServiceResult<PhotoResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<PhotoResponseDto>> CreateAsync(CreatePhotoDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}