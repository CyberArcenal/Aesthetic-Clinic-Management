using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Clients.Repositories
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> GetByEmailAsync(string email);
    }
}