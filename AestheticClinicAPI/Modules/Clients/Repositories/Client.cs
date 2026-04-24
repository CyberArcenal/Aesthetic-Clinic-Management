using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Modules.Shared;
namespace AestheticClinicAPI.Modules.Clients.Repositories
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        public ClientRepository(AppDbContext context) : base(context) { }

        public async Task<Client?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted);
        }
    }
}