using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Billing.DTOs;

namespace AestheticClinicAPI.Modules.Billing.Services
{
    public interface IInvoiceService
    {
        Task<ServiceResult<IEnumerable<InvoiceResponseDto>>> GetAllAsync();
        Task<ServiceResult<InvoiceResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<InvoiceResponseDto>> CreateAsync(CreateInvoiceDto dto);
        Task<ServiceResult<InvoiceResponseDto>> UpdateAsync(int id, UpdateInvoiceDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<bool>> UpdateStatusAsync(int id, string status);
        Task<ServiceResult<IEnumerable<InvoiceResponseDto>>> GetByClientAsync(int clientId);
        Task<ServiceResult<decimal>> GetTotalPaidForInvoiceAsync(int invoiceId);
        Task<ServiceResult<InvoiceResponseDto>> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<ServiceResult<PaginatedResult<InvoiceResponseDto>>> GetPaginatedAsync(int page, int pageSize, int? clientId = null, string? status = null);
    }
}