using System.Linq.Expressions;
using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Modules.Clients.Repositories;
using AestheticClinicAPI.Modules.Shared;

namespace AestheticClinicAPI.Modules.Clients.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepo;

        public ClientService(IClientRepository clientRepo)
        {
            _clientRepo = clientRepo;
        }

        private static ClientResponseDto MapToDto(Client client)
        {
            return new ClientResponseDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                PhoneNumber = client.PhoneNumber,
                DateOfBirth = client.DateOfBirth,
                SkinHistory = client.SkinHistory,
                Allergies = client.Allergies,
                CreatedAt = client.CreatedAt
            };
        }

        public async Task<ServiceResult<IEnumerable<ClientResponseDto>>> GetAllClientsAsync()
        {
            var clients = await _clientRepo.GetAllAsync();
            var dtos = clients.Select(MapToDto);
            return ServiceResult<IEnumerable<ClientResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<ClientResponseDto>> GetClientByIdAsync(int id)
        {
            var client = await _clientRepo.GetByIdAsync(id);
            if (client == null)
                return ServiceResult<ClientResponseDto>.Failure("Client not found.");
            return ServiceResult<ClientResponseDto>.Success(MapToDto(client));
        }

        public async Task<ServiceResult<ClientResponseDto>> GetByIdAsync(int id)
        {
            var client = await _clientRepo.GetByIdAsync(id);
            if (client == null)
                return ServiceResult<ClientResponseDto>.Failure("Client not found.");
            return ServiceResult<ClientResponseDto>.Success(MapToDto(client));
        }

        public async Task<ServiceResult<ClientResponseDto>> CreateClientAsync(CreateClientDto dto)
        {
            var exists = await _clientRepo.ExistsAsync(c => c.Email == dto.Email);
            if (exists)
                return ServiceResult<ClientResponseDto>.Failure("Email already exists.");

            var client = new Client
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                SkinHistory = dto.SkinHistory,
                Allergies = dto.Allergies
            };

            var created = await _clientRepo.AddAsync(client);
            return ServiceResult<ClientResponseDto>.Success(MapToDto(created));
        }

        public async Task<ServiceResult<ClientResponseDto>> UpdateClientAsync(int id, UpdateClientDto dto)
        {
            var client = await _clientRepo.GetByIdAsync(id);
            if (client == null)
                return ServiceResult<ClientResponseDto>.Failure("Client not found.");

            if (!string.IsNullOrEmpty(dto.FirstName))
                client.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName))
                client.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.Email))
                client.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                client.PhoneNumber = dto.PhoneNumber;
            if (dto.DateOfBirth.HasValue)
                client.DateOfBirth = dto.DateOfBirth;
            if (!string.IsNullOrEmpty(dto.SkinHistory))
                client.SkinHistory = dto.SkinHistory;
            if (!string.IsNullOrEmpty(dto.Allergies))
                client.Allergies = dto.Allergies;

            await _clientRepo.UpdateAsync(client);
            return ServiceResult<ClientResponseDto>.Success(MapToDto(client));
        }

        public async Task<ServiceResult<bool>> DeleteClientAsync(int id)
        {
            var client = await _clientRepo.GetByIdAsync(id);
            if (client == null)
                return ServiceResult<bool>.Failure("Client not found.");

            await _clientRepo.DeleteAsync(client);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<PaginatedResult<ClientResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? search = null)
        {
            Expression<Func<Client, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = c => c.FirstName.Contains(search) || c.LastName.Contains(search) || c.Email.Contains(search);
            }
            var paginated = await _clientRepo.GetPaginatedAsync(page, pageSize, filter);
            var dtos = paginated.Items.Select(MapToDto);
            var result = new PaginatedResult<ClientResponseDto>
            {
                Items = dtos,
                Page = paginated.Page,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount
            };
            return ServiceResult<PaginatedResult<ClientResponseDto>>.Success(result);
        }
    }
}