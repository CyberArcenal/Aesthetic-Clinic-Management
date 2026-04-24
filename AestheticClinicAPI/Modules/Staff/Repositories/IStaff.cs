using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Staff.Models;

namespace AestheticClinicAPI.Modules.Staff.Repositories
{
    public interface IStaffRepository : IRepository<StaffMember>
    {
        Task<IEnumerable<StaffMember>> GetActiveAsync();
        Task<IEnumerable<StaffMember>> GetByPositionAsync(string position);
    }
}