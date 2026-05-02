using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Treatments.Models;

namespace AestheticClinicAPI.Modules.Treatments.Repositories
{
    public interface ITreatmentRepository : IRepository<Treatment>
    {
        Task<IEnumerable<Treatment>> GetByCategoryAsync(string category);
        Task<IEnumerable<Treatment>> GetActiveAsync();
    }
}