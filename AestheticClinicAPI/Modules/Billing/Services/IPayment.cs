using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Billing.DTOs;

namespace AestheticClinicAPI.Modules.Billing.Services
{
    public interface IPaymentService
    {
        Task<ServiceResult<IEnumerable<PaymentResponseDto>>> GetAllAsync();
        Task<ServiceResult<PaymentResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<PaymentResponseDto>> CreateAsync(CreatePaymentDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<IEnumerable<PaymentResponseDto>>> GetByInvoiceAsync(int invoiceId);
        Task<ServiceResult<decimal>> GetTotalPaymentsByDateRangeAsync(DateTime start, DateTime end);
        Task<ServiceResult<PaginatedResult<PaymentResponseDto>>> GetPaginatedAsync(int page, int pageSize, int? invoiceId = null, string? method = null);
    }
}