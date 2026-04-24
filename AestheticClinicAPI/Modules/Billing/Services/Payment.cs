using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Billing.DTOs;
using AestheticClinicAPI.Modules.Billing.Models;
using AestheticClinicAPI.Modules.Billing.Repositories;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Modules.Billing.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IInvoiceService _invoiceService;

        public PaymentService(IPaymentRepository paymentRepo, IInvoiceService invoiceService)
        {
            _paymentRepo = paymentRepo;
            _invoiceService = invoiceService;
        }

        private async Task<PaymentResponseDto> MapToDto(Payment payment)
        {
            var invoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
            return new PaymentResponseDto
            {
                Id = payment.Id,
                InvoiceId = payment.InvoiceId,
                InvoiceNumber = invoice.IsSuccess ? invoice.Data?.InvoiceNumber : null,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Method = payment.Method,
                ReferenceNumber = payment.ReferenceNumber,
                Notes = payment.Notes,
                CreatedAt = payment.CreatedAt
            };
        }

        public async Task<ServiceResult<IEnumerable<PaymentResponseDto>>> GetAllAsync()
        {
            var payments = await _paymentRepo.GetAllAsync();
            var dtos = new List<PaymentResponseDto>();
            foreach (var p in payments)
                dtos.Add(await MapToDto(p));
            return ServiceResult<IEnumerable<PaymentResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<PaginatedResult<PaymentResponseDto>>> GetPaginatedAsync(int page, int pageSize, int? invoiceId = null, string? method = null)
        {
            // Build filter expression
            Expression<Func<Payment, bool>>? filter = null;
            if (invoiceId.HasValue || !string.IsNullOrEmpty(method))
            {
                filter = p => (!invoiceId.HasValue || p.InvoiceId == invoiceId.Value)
                           && (string.IsNullOrEmpty(method) || p.Method == method);
            }

            var paginated = await _paymentRepo.GetPaginatedAsync(page, pageSize, filter);
            var dtos = new List<PaymentResponseDto>();
            foreach (var payment in paginated.Items)
                dtos.Add(await MapToDto(payment));

            var result = new PaginatedResult<PaymentResponseDto>
            {
                Items = dtos,
                Page = paginated.Page,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount
            };
            return ServiceResult<PaginatedResult<PaymentResponseDto>>.Success(result);
        }

        public async Task<ServiceResult<PaymentResponseDto>> GetByIdAsync(int id)
        {
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null)
                return ServiceResult<PaymentResponseDto>.Failure("Payment not found.");
            return ServiceResult<PaymentResponseDto>.Success(await MapToDto(payment));
        }

        public async Task<ServiceResult<PaymentResponseDto>> CreateAsync(CreatePaymentDto dto)
        {
            // Validate invoice exists
            var invoiceResult = await _invoiceService.GetByIdAsync(dto.InvoiceId);
            if (!invoiceResult.IsSuccess)
                return ServiceResult<PaymentResponseDto>.Failure("Invoice not found.");

            // Prevent overpayment? (optional check)
            var totalPaid = await _paymentRepo.GetTotalPaymentsByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue); // not efficient; better customize repo method
            // We'll keep simple for now.

            var payment = new Payment
            {
                InvoiceId = dto.InvoiceId,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                Method = dto.Method,
                ReferenceNumber = dto.ReferenceNumber,
                Notes = dto.Notes
            };
            var created = await _paymentRepo.AddAsync(payment);

            // Update invoice status based on total payments
            var totalPaidForInvoice = await _invoiceService.GetTotalPaidForInvoiceAsync(dto.InvoiceId);
            var invoice = invoiceResult.Data;
            if (totalPaidForInvoice.IsSuccess && totalPaidForInvoice.Data >= invoice!.Total)
                await _invoiceService.UpdateStatusAsync(dto.InvoiceId, "Paid");
            else if (totalPaidForInvoice.IsSuccess && totalPaidForInvoice.Data > 0)
                await _invoiceService.UpdateStatusAsync(dto.InvoiceId, "Partial");

            return ServiceResult<PaymentResponseDto>.Success(await MapToDto(created));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null)
                return ServiceResult<bool>.Failure("Payment not found.");
            await _paymentRepo.DeleteAsync(payment);

            // Recalculate invoice status after deletion
            var totalPaid = await _invoiceService.GetTotalPaidForInvoiceAsync(payment.InvoiceId);
            var invoiceResult = await _invoiceService.GetByIdAsync(payment.InvoiceId);
            if (invoiceResult.IsSuccess)
            {
                var invoice = invoiceResult.Data!;
                string newStatus;
                if (totalPaid.IsSuccess && totalPaid.Data <= 0)
                    newStatus = "Sent";
                else if (totalPaid.IsSuccess && totalPaid.Data < invoice.Total)
                    newStatus = "Partial";
                else if (totalPaid.IsSuccess && totalPaid.Data >= invoice.Total)
                    newStatus = "Paid";
                else
                    newStatus = invoice.Status;
                await _invoiceService.UpdateStatusAsync(payment.InvoiceId, newStatus);
            }
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<IEnumerable<PaymentResponseDto>>> GetByInvoiceAsync(int invoiceId)
        {
            var payments = await _paymentRepo.GetByInvoiceAsync(invoiceId);
            var dtos = new List<PaymentResponseDto>();
            foreach (var p in payments)
                dtos.Add(await MapToDto(p));
            return ServiceResult<IEnumerable<PaymentResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<decimal>> GetTotalPaymentsByDateRangeAsync(DateTime start, DateTime end)
        {
            var total = await _paymentRepo.GetTotalPaymentsByDateRangeAsync(start, end);
            return ServiceResult<decimal>.Success(total);
        }
    }
}