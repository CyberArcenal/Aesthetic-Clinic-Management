using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Modules.Dashboard.Services;
using AestheticClinicAPI.Modules.Dashboard.DTOs;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Dashboard.Controllers.v1;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats()
    {
        var stats = await _dashboardService.GetDashboardStatsAsync();
        return Ok(ApiResponse<DashboardStatsDto>.Ok(stats));
    }
}