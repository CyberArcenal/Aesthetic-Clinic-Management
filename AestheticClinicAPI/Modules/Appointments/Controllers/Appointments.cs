using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Appointments.DTOs;
using AestheticClinicAPI.Modules.Appointments.Services;

namespace AestheticClinicAPI.Modules.Appointments.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<AppointmentResponseDto>>>> GetAll(
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10,
       [FromQuery] int? clientId = null,
       [FromQuery] string? status = null,
       [FromQuery] DateTime? fromDate = null,
       [FromQuery] DateTime? toDate = null)
        {
            var result = await _appointmentService.GetPaginatedAsync(page, pageSize, clientId, status, fromDate, toDate);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaginatedResult<AppointmentResponseDto>>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaginatedResult<AppointmentResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> GetById(int id)
        {
            var result = await _appointmentService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<AppointmentResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<AppointmentResponseDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> Create([FromBody] CreateAppointmentDto dto)
        {
            var result = await _appointmentService.CreateAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<AppointmentResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<AppointmentResponseDto>.Ok(result.Data!, "Appointment created."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> Update(int id, [FromBody] UpdateAppointmentDto dto)
        {
            var result = await _appointmentService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<AppointmentResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<AppointmentResponseDto>.Ok(result.Data!, "Appointment updated."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _appointmentService.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Appointment deleted."));
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
        {
            var result = await _appointmentService.UpdateStatusAsync(id, dto.Status);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Status updated."));
        }

        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetByClient(int clientId)
        {
            var result = await _appointmentService.GetByClientAsync(clientId);
            return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("daterange")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetByDateRange(DateTime start, DateTime end)
        {
            var result = await _appointmentService.GetByDateRangeAsync(start, end);
            return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.Ok(result.Data!));
        }
    }
}