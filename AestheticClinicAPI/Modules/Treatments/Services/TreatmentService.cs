using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Treatments.DTOs;
using AestheticClinicAPI.Modules.Treatments.Models;
using AestheticClinicAPI.Modules.Treatments.Repositories;
using AestheticClinicAPI.Modules.Appointments.Repositories;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Modules.Treatments.Services
{
    public class TreatmentService : ITreatmentService
    {
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IAppointmentRepository _appointmentRepository; // optional for revenue

        public TreatmentService(ITreatmentRepository treatmentRepository, IAppointmentRepository appointmentRepository = null)
        {
            _treatmentRepository = treatmentRepository;
            _appointmentRepository = appointmentRepository;
        }

        private static TreatmentResponseDto MapToDto(Treatment treatment)
        {
            return new TreatmentResponseDto
            {
                Id = treatment.Id,
                Name = treatment.Name,
                Description = treatment.Description,
                Category = treatment.Category,
                DurationMinutes = treatment.DurationMinutes,
                Price = treatment.Price,
                IsActive = treatment.IsActive,
                CreatedAt = treatment.CreatedAt
            };
        }

        public async Task<ServiceResult<IEnumerable<TreatmentResponseDto>>> GetAllAsync()
        {
            var treatments = await _treatmentRepository.GetAllAsync();
            var dtos = treatments.Select(MapToDto);
            return ServiceResult<IEnumerable<TreatmentResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<TreatmentResponseDto>> GetByIdAsync(int id)
        {
            var treatment = await _treatmentRepository.GetByIdAsync(id);
            if (treatment == null)
                return ServiceResult<TreatmentResponseDto>.Failure("Treatment not found.");
            return ServiceResult<TreatmentResponseDto>.Success(MapToDto(treatment));
        }

        public async Task<ServiceResult<TreatmentResponseDto>> CreateAsync(CreateTreatmentDto dto)
        {
            var treatment = new Treatment
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                DurationMinutes = dto.DurationMinutes,
                Price = dto.Price,
                IsActive = dto.IsActive
            };
            var created = await _treatmentRepository.AddAsync(treatment);
            return ServiceResult<TreatmentResponseDto>.Success(MapToDto(created));
        }

        public async Task<ServiceResult<TreatmentResponseDto>> UpdateAsync(int id, UpdateTreatmentDto dto)
        {
            var treatment = await _treatmentRepository.GetByIdAsync(id);
            if (treatment == null)
                return ServiceResult<TreatmentResponseDto>.Failure("Treatment not found.");

            if (!string.IsNullOrEmpty(dto.Name))
                treatment.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description))
                treatment.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.Category))
                treatment.Category = dto.Category;
            if (dto.DurationMinutes.HasValue)
                treatment.DurationMinutes = dto.DurationMinutes.Value;
            if (dto.Price.HasValue)
                treatment.Price = dto.Price.Value;
            if (dto.IsActive.HasValue)
                treatment.IsActive = dto.IsActive.Value;

            await _treatmentRepository.UpdateAsync(treatment);
            return ServiceResult<TreatmentResponseDto>.Success(MapToDto(treatment));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var treatment = await _treatmentRepository.GetByIdAsync(id);
            if (treatment == null)
                return ServiceResult<bool>.Failure("Treatment not found.");
            // Optional: check if any appointment uses this treatment before deleting
            await _treatmentRepository.DeleteAsync(treatment);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> ToggleActiveAsync(int id)
        {
            var treatment = await _treatmentRepository.GetByIdAsync(id);
            if (treatment == null)
                return ServiceResult<bool>.Failure("Treatment not found.");
            treatment.IsActive = !treatment.IsActive;
            await _treatmentRepository.UpdateAsync(treatment);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<IEnumerable<TreatmentResponseDto>>> GetByCategoryAsync(string category)
        {
            var treatments = await _treatmentRepository.GetByCategoryAsync(category);
            var dtos = treatments.Select(MapToDto);
            return ServiceResult<IEnumerable<TreatmentResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IEnumerable<TreatmentResponseDto>>> GetActiveAsync()
        {
            var treatments = await _treatmentRepository.GetActiveAsync();
            var dtos = treatments.Select(MapToDto);
            return ServiceResult<IEnumerable<TreatmentResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<decimal>> GetTotalRevenueAsync()
        {
            // Require appointment repository to sum completed appointments per treatment
            if (_appointmentRepository == null)
                return ServiceResult<decimal>.Failure("Appointment repository not available.");
            var completedAppointments = await _appointmentRepository.GetByStatusAsync("Completed");
            var total = completedAppointments
                .Where(a => a.TreatmentId != 0)
                .Join((await _treatmentRepository.GetAllAsync()), a => a.TreatmentId, t => t.Id, (a, t) => t.Price)
                .Sum();
            return ServiceResult<decimal>.Success(total);
        }

        public async Task<ServiceResult<PaginatedResult<TreatmentResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? search = null)
        {
            Expression<Func<Treatment, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = t => t.Name.Contains(search) || (t.Category != null && t.Category.Contains(search));
            }
            var paginated = await _treatmentRepository.GetPaginatedAsync(page, pageSize, filter);
            var dtos = paginated.Items.Select(MapToDto);
            var result = new PaginatedResult<TreatmentResponseDto>
            {
                Items = dtos,
                Page = paginated.Page,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount
            };
            return ServiceResult<PaginatedResult<TreatmentResponseDto>>.Success(result);
        }
    }
}