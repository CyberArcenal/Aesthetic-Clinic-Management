using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.Repositories;

namespace AestheticClinicAPI.Modules.Authentications.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepo;

        public RoleService(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        private static RoleResponseDto MapToDto(Role role) => new()
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };

        public async Task<ServiceResult<IEnumerable<RoleResponseDto>>> GetAllAsync()
        {
            var roles = await _roleRepo.GetAllAsync();
            var dtos = roles.Select(MapToDto);
            return ServiceResult<IEnumerable<RoleResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<RoleResponseDto>> GetByIdAsync(int id)
        {
            var role = await _roleRepo.GetByIdAsync(id);
            if (role == null)
                return ServiceResult<RoleResponseDto>.Failure("Role not found.");
            return ServiceResult<RoleResponseDto>.Success(MapToDto(role));
        }

        public async Task<ServiceResult<RoleResponseDto>> GetByNameAsync(string name)
        {
            var role = await _roleRepo.GetByNameAsync(name);
            if (role == null)
                return ServiceResult<RoleResponseDto>.Failure("Role not found.");
            return ServiceResult<RoleResponseDto>.Success(MapToDto(role));
        }

        public async Task<ServiceResult<RoleResponseDto>> CreateAsync(CreateRoleDto dto)
        {
            if (await _roleRepo.GetByNameAsync(dto.Name) != null)
                return ServiceResult<RoleResponseDto>.Failure("Role name already exists.");

            var role = new Role { Name = dto.Name, Description = dto.Description };
            var created = await _roleRepo.AddAsync(role);
            return ServiceResult<RoleResponseDto>.Success(MapToDto(created));
        }

        public async Task<ServiceResult<RoleResponseDto>> UpdateAsync(int id, UpdateRoleDto dto)
        {
            var role = await _roleRepo.GetByIdAsync(id);
            if (role == null)
                return ServiceResult<RoleResponseDto>.Failure("Role not found.");

            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != role.Name)
            {
                if (await _roleRepo.GetByNameAsync(dto.Name) != null)
                    return ServiceResult<RoleResponseDto>.Failure("Role name already exists.");
                role.Name = dto.Name;
            }
            if (dto.Description != null)
                role.Description = dto.Description;

            await _roleRepo.UpdateAsync(role);
            return ServiceResult<RoleResponseDto>.Success(MapToDto(role));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var role = await _roleRepo.GetByIdAsync(id);
            if (role == null)
                return ServiceResult<bool>.Failure("Role not found.");

            // Optional: Check if any user has this role before deleting
            // (You can add this logic if needed)
            await _roleRepo.DeleteAsync(role);
            return ServiceResult<bool>.Success(true);
        }
    }
}