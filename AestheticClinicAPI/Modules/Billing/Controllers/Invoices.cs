using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Billing.DTOs;
using AestheticClinicAPI.Modules.Billing.Services;

namespace AestheticClinicAPI.Modules.Billing.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<InvoiceResponseDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? clientId = null, [FromQuery] string? status = null)
        {
            var result = await _invoiceService.GetPaginatedAsync(page, pageSize, clientId, status);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaginatedResult<InvoiceResponseDto>>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaginatedResult<InvoiceResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> GetById(int id)
        {
            var result = await _invoiceService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<InvoiceResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<InvoiceResponseDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> Create([FromBody] CreateInvoiceDto dto)
        {
            var result = await _invoiceService.CreateAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<InvoiceResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<InvoiceResponseDto>.Ok(result.Data!, "Invoice created."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> Update(int id, [FromBody] UpdateInvoiceDto dto)
        {
            var result = await _invoiceService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<InvoiceResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<InvoiceResponseDto>.Ok(result.Data!, "Invoice updated."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _invoiceService.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Invoice deleted."));
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusDto dto)
        {
            var result = await _invoiceService.UpdateStatusAsync(id, dto.Status);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Invoice status updated."));
        }

        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceResponseDto>>>> GetByClient(int clientId)
        {
            var result = await _invoiceService.GetByClientAsync(clientId);
            return Ok(ApiResponse<IEnumerable<InvoiceResponseDto>>.Ok(result.Data!));
        }
    }
}