using AestheticClinicAPI.Shared;
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
        private readonly IWebHostEnvironment _env;

        public PhotoService(IPhotoRepository photoRepo, IClientService clientService, IWebHostEnvironment env)
        {
            _photoRepo = photoRepo;
            _clientService = clientService;
            _env = env;
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
            // Validate client
            var clientCheck = await _clientService.GetByIdAsync(dto.ClientId);
            if (!clientCheck.IsSuccess)
                return ServiceResult<PhotoResponseDto>.Failure("Client not found.");

            // 1. Save file to disk
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "photos");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var originalFileName = Path.GetFileName(dto.File.FileName);
            var fileExtension = Path.GetExtension(originalFileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            // 2. Create entity
            var photo = new Photo
            {
                ClientId = dto.ClientId,
                AppointmentId = dto.AppointmentId,
                IsBefore = dto.IsBefore,
                Description = dto.Description,
                FileName = originalFileName,
                FilePath = $"/uploads/photos/{uniqueFileName}", // relative URL
                FileSize = dto.File.Length,
                MimeType = dto.File.ContentType
            };

            var created = await _photoRepo.AddAsync(photo);
            return ServiceResult<PhotoResponseDto>.Success(await MapToDto(created));
        }

        public async Task<ServiceResult<PhotoFileDto>> GetPhotoFileAsync(int id)
        {
            var photo = await _photoRepo.GetByIdAsync(id);
            if (photo == null)
                return ServiceResult<PhotoFileDto>.Failure("Photo not found.");

            var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var physicalPath = Path.Combine(wwwroot, photo.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(physicalPath))
                return ServiceResult<PhotoFileDto>.Failure("File not found on disk.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
            return ServiceResult<PhotoFileDto>.Success(new PhotoFileDto
            {
                FileBytes = fileBytes,
                MimeType = photo.MimeType ?? "application/octet-stream",
                FileName = photo.FileName
            });
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var photo = await _photoRepo.GetByIdAsync(id);
            if (photo == null)
                return ServiceResult<bool>.Failure("Photo not found.");

            // Delete physical file
            var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var physicalPath = Path.Combine(wwwroot, photo.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            await _photoRepo.DeleteAsync(photo);
            return ServiceResult<bool>.Success(true);
        }

    }
}