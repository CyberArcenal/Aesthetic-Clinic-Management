using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Billing.DTOs;
using AestheticClinicAPI.Modules.Billing.Services;

namespace AestheticClinicAPI.Modules.Billing.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<PaymentResponseDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? invoiceId = null,
        [FromQuery] string? method = null)
        {
            var result = await _paymentService.GetPaginatedAsync(page, pageSize, invoiceId, method);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaginatedResult<PaymentResponseDto>>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaginatedResult<PaymentResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> GetById(int id)
        {
            var result = await _paymentService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<PaymentResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaymentResponseDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> Create([FromBody] CreatePaymentDto dto)
        {
            var result = await _paymentService.CreateAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaymentResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaymentResponseDto>.Ok(result.Data!, "Payment recorded."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _paymentService.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Payment deleted."));
        }

        [HttpGet("invoice/{invoiceId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentResponseDto>>>> GetByInvoice(int invoiceId)
        {
            var result = await _paymentService.GetByInvoiceAsync(invoiceId);
            return Ok(ApiResponse<IEnumerable<PaymentResponseDto>>.Ok(result.Data!));
        }
    }
}