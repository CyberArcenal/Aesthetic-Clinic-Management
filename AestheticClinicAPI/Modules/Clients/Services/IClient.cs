using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Modules.Shared;

namespace AestheticClinicAPI.Modules.Clients.Services
{
    public interface IClientService
    {
        Task<ServiceResult<IEnumerable<ClientResponseDto>>> GetAllClientsAsync();
        Task<ServiceResult<ClientResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<ClientResponseDto>> GetClientByIdAsync(int id);
        Task<ServiceResult<ClientResponseDto>> CreateClientAsync(CreateClientDto dto);
        Task<ServiceResult<ClientResponseDto>> UpdateClientAsync(int id, UpdateClientDto dto);
        Task<ServiceResult<bool>> DeleteClientAsync(int id);

        Task<ServiceResult<PaginatedResult<ClientResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? search = null);
    }
}