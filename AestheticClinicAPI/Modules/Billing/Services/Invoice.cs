using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Billing.DTOs;
using AestheticClinicAPI.Modules.Billing.Models;
using AestheticClinicAPI.Modules.Billing.Repositories;
using AestheticClinicAPI.Modules.Clients.Services;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Modules.Billing.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IClientService _clientService;

        public InvoiceService(IInvoiceRepository invoiceRepo, IClientService clientService)
        {
            _invoiceRepo = invoiceRepo;
            _clientService = clientService;
        }

        private async Task<InvoiceResponseDto> MapToDto(Invoice invoice)
        {
            var clientResult = await _clientService.GetByIdAsync(invoice.ClientId);
            var totalPaid = await _invoiceRepo.GetTotalPaidByInvoiceAsync(invoice.Id);

            return new InvoiceResponseDto
            {
                Id = invoice.Id,
                ClientId = invoice.ClientId,
                ClientName = clientResult.IsSuccess ? clientResult.Data?.FullName : null,
                AppointmentId = invoice.AppointmentId,
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                Subtotal = invoice.Subtotal,
                Tax = invoice.Tax,
                Total = invoice.Total,
                Status = invoice.Status,
                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt,
                AmountPaid = totalPaid
            };
        }

        public async Task<ServiceResult<IEnumerable<InvoiceResponseDto>>> GetAllAsync()
        {
            var invoices = await _invoiceRepo.GetAllAsync();
            var dtos = new List<InvoiceResponseDto>();
            foreach (var inv in invoices)
                dtos.Add(await MapToDto(inv));
            return ServiceResult<IEnumerable<InvoiceResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<PaginatedResult<InvoiceResponseDto>>> GetPaginatedAsync(int page, int pageSize, int? clientId = null, string? status = null)
        {
            Expression<Func<Invoice, bool>>? filter = null;
            if (clientId.HasValue || !string.IsNullOrEmpty(status))
            {
                filter = i => (!clientId.HasValue || i.ClientId == clientId.Value)
                           && (string.IsNullOrEmpty(status) || i.Status == status);
            }
            var paginated = await _invoiceRepo.GetPaginatedAsync(page, pageSize, filter);
            var dtos = new List<InvoiceResponseDto>();
            foreach (var inv in paginated.Items)
                dtos.Add(await MapToDto(inv));
            var result = new PaginatedResult<InvoiceResponseDto>
            {
                Items = dtos,
                Page = paginated.Page,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount
            };
            return ServiceResult<PaginatedResult<InvoiceResponseDto>>.Success(result);
        }

        public async Task<ServiceResult<InvoiceResponseDto>> GetByIdAsync(int id)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(id);
            if (invoice == null)
                return ServiceResult<InvoiceResponseDto>.Failure("Invoice not found.");
            return ServiceResult<InvoiceResponseDto>.Success(await MapToDto(invoice));
        }

        public async Task<ServiceResult<InvoiceResponseDto>> CreateAsync(CreateInvoiceDto dto)
        {
            // Generate invoice number
            var invoiceNumber = GenerateInvoiceNumber();

            var invoice = new Invoice
            {
                ClientId = dto.ClientId,
                AppointmentId = dto.AppointmentId,
                InvoiceNumber = invoiceNumber,
                IssueDate = dto.IssueDate,
                DueDate = dto.DueDate,
                Subtotal = dto.Subtotal,
                Tax = dto.Tax,
                Total = dto.Subtotal + dto.Tax,
                Status = "Draft",
                Notes = dto.Notes
            };
            var created = await _invoiceRepo.AddAsync(invoice);
            return ServiceResult<InvoiceResponseDto>.Success(await MapToDto(created));
        }

        public async Task<ServiceResult<InvoiceResponseDto>> UpdateAsync(int id, UpdateInvoiceDto dto)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(id);
            if (invoice == null)
                return ServiceResult<InvoiceResponseDto>.Failure("Invoice not found.");

            if (dto.IssueDate.HasValue) invoice.IssueDate = dto.IssueDate.Value;
            if (dto.DueDate.HasValue) invoice.DueDate = dto.DueDate.Value;
            if (dto.Subtotal.HasValue) invoice.Subtotal = dto.Subtotal.Value;
            if (dto.Tax.HasValue) invoice.Tax = dto.Tax.Value;
            if (dto.Subtotal.HasValue || dto.Tax.HasValue)
                invoice.Total = invoice.Subtotal + invoice.Tax;
            if (!string.IsNullOrEmpty(dto.Notes)) invoice.Notes = dto.Notes;

            await _invoiceRepo.UpdateAsync(invoice);
            return ServiceResult<InvoiceResponseDto>.Success(await MapToDto(invoice));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(id);
            if (invoice == null)
                return ServiceResult<bool>.Failure("Invoice not found.");
            if (invoice.Status == "Paid")
                return ServiceResult<bool>.Failure("Cannot delete a paid invoice.");
            await _invoiceRepo.DeleteAsync(invoice);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> UpdateStatusAsync(int id, string status)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(id);
            if (invoice == null)
                return ServiceResult<bool>.Failure("Invoice not found.");
            invoice.Status = status;
            await _invoiceRepo.UpdateAsync(invoice);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<IEnumerable<InvoiceResponseDto>>> GetByClientAsync(int clientId)
        {
            var invoices = await _invoiceRepo.GetByClientAsync(clientId);
            var dtos = new List<InvoiceResponseDto>();
            foreach (var inv in invoices)
                dtos.Add(await MapToDto(inv));
            return ServiceResult<IEnumerable<InvoiceResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<decimal>> GetTotalPaidForInvoiceAsync(int invoiceId)
        {
            var total = await _invoiceRepo.GetTotalPaidByInvoiceAsync(invoiceId);
            return ServiceResult<decimal>.Success(total);
        }

        public async Task<ServiceResult<InvoiceResponseDto>> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            var invoice = await _invoiceRepo.GetByInvoiceNumberAsync(invoiceNumber);
            if (invoice == null)
                return ServiceResult<InvoiceResponseDto>.Failure("Invoice not found.");
            return ServiceResult<InvoiceResponseDto>.Success(await MapToDto(invoice));
        }

        private string GenerateInvoiceNumber()
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month;
            var count = _invoiceRepo.CountAsync().Result + 1;
            return $"INV-{year}{month:D2}-{count:D4}";
        }
    }
}