using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Photos.DTOs;
using AestheticClinicAPI.Modules.Photos.Models;
using AestheticClinicAPI.Modules.Photos.Repositories;
using AestheticClinicAPI.Modules.Clients.Services;

namespace AestheticClinicAPI.Modules.Photos.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IPhotoRepository _photoRepo;
        private readonly IClientService _clientService;

        public PhotoService(IPhotoRepository photoRepo, IClientService clientService)
        {
            _photoRepo = photoRepo;
            _clientService = clientService;
        }

        private async Task<PhotoResponseDto> MapToDto(Photo photo)
        {
            string? clientName = null;
            var clientResult = await _clientService.GetByIdAsync(photo.ClientId);
            if (clientResult.IsSuccess)
                clientName = clientResult.Data?.FullName;

            return new PhotoResponseDto
            {
                Id = photo.Id,
                ClientId = photo.ClientId,
                ClientName = clientName,
                AppointmentId = photo.AppointmentId,
                FileName = photo.FileName,
                FilePath = photo.FilePath,
                Description = photo.Description,
                IsBefore = photo.IsBefore,
                FileSize = photo.FileSize,
                MimeType = photo.MimeType,
                CreatedAt = photo.CreatedAt
            };
        }

        public async Task<ServiceResult<IEnumerable<PhotoResponseDto>>> GetByClientAsync(int clientId)
        {
            var photos = await _photoRepo.GetByClientAsync(clientId);
            var dtos = new List<PhotoResponseDto>();
            foreach (var photo in photos)
                dtos.Add(await MapToDto(photo));
            return ServiceResult<IEnumerable<PhotoResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IEnumerable<PhotoResponseDto>>> GetByAppointmentAsync(int appointmentId)
        {
            var photos = await _photoRepo.GetByAppointmentAsync(appointmentId);
            var dtos = new List<PhotoResponseDto>();
            foreach (var photo in photos)
                dtos.Add(await MapToDto(photo));
            return ServiceResult<IEnumerable<PhotoResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IEnumerable<PhotoResponseDto>>> GetBeforePhotosAsync(int clientId)
        {
            var photos = await _photoRepo.GetBeforePhotosAsync(clientId);
            var dtos = new List<PhotoResponseDto>();
            foreach (var photo in photos)
                dtos.Add(await MapToDto(photo));
            return ServiceResult<IEnumerable<PhotoResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IEnumerable<PhotoResponseDto>>> GetAfterPhotosAsync(int clientId)
        {
            var photos = await _photoRepo.GetAfterPhotosAsync(clientId);
            var dtos = new List<PhotoResponseDto>();
            foreach (var photo in photos)
                dtos.Add(await MapToDto(photo));
            return ServiceResult<IEnumerable<PhotoResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<PhotoResponseDto>> GetByIdAsync(int id)
        {
            var photo = await _photoRepo.GetByIdAsync(id);
            if (photo == null)
                return ServiceResult<PhotoResponseDto>.Failure("Photo not found.");
            return ServiceResult<PhotoResponseDto>.Success(await MapToDto(photo));
        }

        public async Task<ServiceResult<PhotoResponseDto>> CreateAsync(CreatePhotoDto dto)
        {
            var photo = new Photo
            {
                ClientId = dto.ClientId,
                AppointmentId = dto.AppointmentId,
                FileName = dto.FileName,
                FilePath = dto.FilePath,
                Description = dto.Description,
                IsBefore = dto.IsBefore,
                FileSize = dto.FileSize,
                MimeType = dto.MimeType
            };
            var created = await _photoRepo.AddAsync(photo);
            return ServiceResult<PhotoResponseDto>.Success(await MapToDto(created));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var photo = await _photoRepo.GetByIdAsync(id);
            if (photo == null)
                return ServiceResult<bool>.Failure("Photo not found.");
            await _photoRepo.DeleteAsync(photo);
            return ServiceResult<bool>.Success(true);
        }
        
    }
}