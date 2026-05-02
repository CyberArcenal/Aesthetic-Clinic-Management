using System.Linq.Expressions;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Staff.DTOs;
using AestheticClinicAPI.Modules.Staff.Models;
using AestheticClinicAPI.Modules.Staff.Repositories;

namespace AestheticClinicAPI.Modules.Staff.Services
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepo;

        public StaffService(IStaffRepository staffRepo)
        {
            _staffRepo = staffRepo;
        }

        private static StaffResponseDto MapToDto(StaffMember staff) => new()
        {
            Id = staff.Id,
            Name = staff.Name,
            Email = staff.Email,
            Phone = staff.Phone,
            Position = staff.Position,
            IsActive = staff.IsActive,
            CreatedAt = staff.CreatedAt
        };

        public async Task<ServiceResult<IEnumerable<StaffResponseDto>>> GetAllAsync()
        {
            var staff = await _staffRepo.GetAllAsync();
            var dtos = staff.Select(MapToDto);
            return ServiceResult<IEnumerable<StaffResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<StaffResponseDto>> GetByIdAsync(int id)
        {
            var staff = await _staffRepo.GetByIdAsync(id);
            if (staff == null)
                return ServiceResult<StaffResponseDto>.Failure("Staff not found.");
            return ServiceResult<StaffResponseDto>.Success(MapToDto(staff));
        }

        public async Task<ServiceResult<StaffResponseDto>> CreateAsync(CreateStaffDto dto)
        {
            var staff = new StaffMember
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Position = dto.Position,
                IsActive = dto.IsActive
            };
            var created = await _staffRepo.AddAsync(staff);
            return ServiceResult<StaffResponseDto>.Success(MapToDto(created));
        }

        public async Task<ServiceResult<StaffResponseDto>> UpdateAsync(int id, UpdateStaffDto dto)
        {
            var staff = await _staffRepo.GetByIdAsync(id);
            if (staff == null)
                return ServiceResult<StaffResponseDto>.Failure("Staff not found.");

            if (!string.IsNullOrEmpty(dto.Name)) staff.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Email)) staff.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Phone)) staff.Phone = dto.Phone;
            if (!string.IsNullOrEmpty(dto.Position)) staff.Position = dto.Position;
            if (dto.IsActive.HasValue) staff.IsActive = dto.IsActive.Value;

            await _staffRepo.UpdateAsync(staff);
            return ServiceResult<StaffResponseDto>.Success(MapToDto(staff));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var staff = await _staffRepo.GetByIdAsync(id);
            if (staff == null) return ServiceResult<bool>.Failure("Staff not found.");
            await _staffRepo.DeleteAsync(staff);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> ToggleActiveAsync(int id)
        {
            var staff = await _staffRepo.GetByIdAsync(id);
            if (staff == null) return ServiceResult<bool>.Failure("Staff not found.");
            staff.IsActive = !staff.IsActive;
            await _staffRepo.UpdateAsync(staff);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<IEnumerable<StaffResponseDto>>> GetActiveAsync()
        {
            var staff = await _staffRepo.GetActiveAsync();
            var dtos = staff.Select(MapToDto);
            return ServiceResult<IEnumerable<StaffResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IEnumerable<StaffResponseDto>>> GetByPositionAsync(string position)
        {
            var staff = await _staffRepo.GetByPositionAsync(position);
            var dtos = staff.Select(MapToDto);
            return ServiceResult<IEnumerable<StaffResponseDto>>.Success(dtos);
        }
        public async Task<ServiceResult<PaginatedResult<StaffResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? search = null)
        {
            Expression<Func<StaffMember, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = s => s.Name.Contains(search) || (s.Position != null && s.Position.Contains(search));
            }
            var paginated = await _staffRepo.GetPaginatedAsync(page, pageSize, filter);
            var dtos = paginated.Items.Select(MapToDto);
            var result = new PaginatedResult<StaffResponseDto>
            {
                Items = dtos,
                Page = paginated.Page,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount
            };
            return ServiceResult<PaginatedResult<StaffResponseDto>>.Success(result);
        }
    }
}