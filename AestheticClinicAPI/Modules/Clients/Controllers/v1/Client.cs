using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Modules.Clients.Services;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Clients.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ClientResponseDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _clientService.GetPaginatedAsync(page, pageSize, search);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaginatedResult<ClientResponseDto>>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaginatedResult<ClientResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ClientResponseDto>>> GetById(int id)
        {
            var result = await _clientService.GetClientByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<ClientResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<ClientResponseDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ClientResponseDto>>> Create([FromBody] CreateClientDto dto)
        {
            var result = await _clientService.CreateClientAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<ClientResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<ClientResponseDto>.Ok(result.Data!, "Client created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ClientResponseDto>>> Update(int id, [FromBody] UpdateClientDto dto)
        {
            var result = await _clientService.UpdateClientAsync(id, dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<ClientResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<ClientResponseDto>.Ok(result.Data!, "Client updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _clientService.DeleteClientAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Client deleted successfully."));
        }
    }
}