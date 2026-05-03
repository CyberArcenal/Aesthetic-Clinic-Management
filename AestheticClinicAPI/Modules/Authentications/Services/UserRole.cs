using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.Repositories;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Authentications.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;

        public UserRoleService(
            IUserRoleRepository userRoleRepo,
            IUserRepository userRepo,
            IRoleRepository roleRepo
        )
        {
            _userRoleRepo = userRoleRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
        }

        private async Task<UserRoleResponseDto> MapToDto(UserRole userRole)
        {
            var user = await _userRepo.GetByIdAsync(userRole.UserId);
            var role = await _roleRepo.GetByIdAsync(userRole.RoleId);
            return new UserRoleResponseDto
            {
                UserId = userRole.UserId,
                Username = user?.Username,
                RoleId = userRole.RoleId,
                RoleName = role?.Name,
            };
        }

        public async Task<ServiceResult<IEnumerable<UserRoleResponseDto>>> GetAllAsync()
        {
            var userRoles = await _userRoleRepo.GetAllAsync();
            var dtos = new List<UserRoleResponseDto>();
            foreach (var ur in userRoles)
                dtos.Add(await MapToDto(ur));
            return ServiceResult<IEnumerable<UserRoleResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IEnumerable<UserRoleResponseDto>>> GetByUserIdAsync(
            int userId
        )
        {
            var userRoles = await _userRoleRepo.GetByUserIdAsync(userId);
            var dtos = new List<UserRoleResponseDto>();
            foreach (var ur in userRoles)
                dtos.Add(await MapToDto(ur));
            return ServiceResult<IEnumerable<UserRoleResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IEnumerable<UserRoleResponseDto>>> GetByRoleIdAsync(
            int roleId
        )
        {
            var userRoles = await _userRoleRepo.GetByRoleIdAsync(roleId);
            var dtos = new List<UserRoleResponseDto>();
            foreach (var ur in userRoles)
                dtos.Add(await MapToDto(ur));
            return ServiceResult<IEnumerable<UserRoleResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<bool>> AssignRoleAsync(AssignRoleDto dto)
        {
            var user = await _userRepo.GetByIdAsync(dto.UserId);
            if (user == null)
                return ServiceResult<bool>.Failure("User not found.");
            var role = await _roleRepo.GetByIdAsync(dto.RoleId);
            if (role == null)
                return ServiceResult<bool>.Failure("Role not found.");

            var exists = await _userRoleRepo.UserHasRoleAsync(dto.UserId, role.Name);
            if (exists)
                return ServiceResult<bool>.Failure("User already has this role.");

            var userRole = new UserRole { UserId = dto.UserId, RoleId = dto.RoleId };
            await _userRoleRepo.AddAsync(userRole);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> RemoveRoleAsync(AssignRoleDto dto)
        {
            var userRole = (await _userRoleRepo.GetByUserIdAsync(dto.UserId)).FirstOrDefault(ur =>
                ur.RoleId == dto.RoleId
            );
            if (userRole == null)
                return ServiceResult<bool>.Failure("User does not have this role.");
            await _userRoleRepo.DeleteAsync(userRole);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> UserHasRoleAsync(int userId, string roleName)
        {
            var hasRole = await _userRoleRepo.UserHasRoleAsync(userId, roleName);
            return ServiceResult<bool>.Success(hasRole);
        }

        public async Task<ServiceResult<IEnumerable<string>>> GetUserRolesAsync(int userId)
        {
            var roles = await _userRoleRepo.GetUserRolesAsync(userId);
            return ServiceResult<IEnumerable<string>>.Success(roles);
        }
    }
}
