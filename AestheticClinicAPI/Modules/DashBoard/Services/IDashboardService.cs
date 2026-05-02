using AestheticClinicAPI.Modules.Dashboard.DTOs;

namespace AestheticClinicAPI.Modules.Dashboard.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}