using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.Repositories;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Modules.Authentications.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IRoleRepository _roleRepo;

        public UserService(IUserRepository userRepo, IUserRoleRepository userRoleRepo, IRoleRepository roleRepo)
        {
            _userRepo = userRepo;
            _userRoleRepo = userRoleRepo;
            _roleRepo = roleRepo;
        }

        private static UserResponseDto MapToDto(User user, IEnumerable<string> roles) => new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            Roles = roles
        };

        public async Task<ServiceResult<IEnumerable<UserResponseDto>>> GetAllAsync()
        {
            var users = await _userRepo.GetAllAsync();
            var dtos = new List<UserResponseDto>();
            foreach (var user in users)
            {
                var roles = await _userRoleRepo.GetUserRolesAsync(user.Id);
                dtos.Add(MapToDto(user, roles));
            }
            return ServiceResult<IEnumerable<UserResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<PaginatedResult<UserResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? search = null)
        {
            Expression<Func<User, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = u => u.Username.Contains(search) || u.Email.Contains(search) || (u.FullName != null && u.FullName.Contains(search));
            }
            var paginated = await _userRepo.GetPaginatedAsync(page, pageSize, filter);
            var dtos = new List<UserResponseDto>();
            foreach (var user in paginated.Items)
            {
                var roles = await _userRoleRepo.GetUserRolesAsync(user.Id);
                dtos.Add(MapToDto(user, roles));
            }
            var result = new PaginatedResult<UserResponseDto>
            {
                Items = dtos,
                Page = paginated.Page,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount
            };
            return ServiceResult<PaginatedResult<UserResponseDto>>.Success(result);
        }

        public async Task<ServiceResult<UserResponseDto>> GetByIdAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return ServiceResult<UserResponseDto>.Failure("User not found.");
            var roles = await _userRoleRepo.GetUserRolesAsync(id);
            return ServiceResult<UserResponseDto>.Success(MapToDto(user, roles));
        }

        public async Task<ServiceResult<UserResponseDto>> CreateAsync(CreateUserDto dto)
        {
            if (await _userRepo.GetByUsernameAsync(dto.Username) != null)
                return ServiceResult<UserResponseDto>.Failure("Username already exists.");
            if (await _userRepo.GetByEmailAsync(dto.Email) != null)
                return ServiceResult<UserResponseDto>.Failure("Email already exists.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                IsActive = dto.IsActive
            };
            var created = await _userRepo.AddAsync(user);

            if (dto.Roles != null && dto.Roles.Any())
            {
                foreach (var roleName in dto.Roles)
                {
                    var role = await _roleRepo.GetByNameAsync(roleName);
                    if (role != null)
                        await _userRoleRepo.AddAsync(new UserRole { UserId = created.Id, RoleId = role.Id });
                }
            }

            var roles = await _userRoleRepo.GetUserRolesAsync(created.Id);
            return ServiceResult<UserResponseDto>.Success(MapToDto(created, roles));
        }

        public async Task<ServiceResult<UserResponseDto>> UpdateAsync(int id, UpdateUserDto dto)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return ServiceResult<UserResponseDto>.Failure("User not found.");

            if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
            {
                if (await _userRepo.GetByUsernameAsync(dto.Username) != null)
                    return ServiceResult<UserResponseDto>.Failure("Username already taken.");
                user.Username = dto.Username;
            }
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                if (await _userRepo.GetByEmailAsync(dto.Email) != null)
                    return ServiceResult<UserResponseDto>.Failure("Email already registered.");
                user.Email = dto.Email;
            }
            if (dto.FullName != null)
                user.FullName = dto.FullName;
            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            await _userRepo.UpdateAsync(user);

            // Update roles
            if (dto.RolesToAdd != null && dto.RolesToAdd.Any())
            {
                foreach (var roleName in dto.RolesToAdd)
                {
                    var role = await _roleRepo.GetByNameAsync(roleName);
                    if (role != null && !await _userRoleRepo.UserHasRoleAsync(id, roleName))
                        await _userRoleRepo.AddAsync(new UserRole { UserId = id, RoleId = role.Id });
                }
            }
            if (dto.RolesToRemove != null && dto.RolesToRemove.Any())
            {
                foreach (var roleName in dto.RolesToRemove)
                {
                    var role = await _roleRepo.GetByNameAsync(roleName);
                    if (role != null)
                    {
                        var userRole = (await _userRoleRepo.GetByUserIdAsync(id))
                            .FirstOrDefault(ur => ur.RoleId == role.Id);
                        if (userRole != null)
                            await _userRoleRepo.DeleteAsync(userRole);
                    }
                }
            }

            var roles = await _userRoleRepo.GetUserRolesAsync(id);
            return ServiceResult<UserResponseDto>.Success(MapToDto(user, roles));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return ServiceResult<bool>.Failure("User not found.");
            await _userRepo.DeleteAsync(user);
            // Optionally also delete related user roles (cascading soft delete is handled by QueryFilter)
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> ActivateAsync(int id, bool isActive)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return ServiceResult<bool>.Failure("User not found.");
            user.IsActive = isActive;
            await _userRepo.UpdateAsync(user);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<IEnumerable<string>>> GetUserRolesAsync(int userId)
        {
            var roles = await _userRoleRepo.GetUserRolesAsync(userId);
            return ServiceResult<IEnumerable<string>>.Success(roles);
        }
    }
}