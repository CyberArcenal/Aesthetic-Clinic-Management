using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Appointments.DTOs;
using AestheticClinicAPI.Modules.Appointments.Models;
using AestheticClinicAPI.Modules.Appointments.Repositories;
using AestheticClinicAPI.Modules.Appointments.Constants;
using AestheticClinicAPI.Modules.Appointments.StateTransitionService;
using AestheticClinicAPI.Modules.Clients.Services;
using AestheticClinicAPI.Modules.Treatments.Services;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Modules.Appointments.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly ITreatmentService _treatmentService;
        private readonly IClientService _clientService;
        private readonly AppointmentStateTransition _stateTransition;

        public AppointmentService(
            IAppointmentRepository appointmentRepo,
            ITreatmentService treatmentService,
            IClientService clientService,
            AppointmentStateTransition stateTransition)
        {
            _appointmentRepo = appointmentRepo;
            _treatmentService = treatmentService;
            _clientService = clientService;
            _stateTransition = stateTransition;
        }

        private async Task<AppointmentResponseDto> EnrichDto(Appointment appointment)
        {
            var clientResult = await _clientService.GetClientByIdAsync(appointment.ClientId);
            var treatmentResult = await _treatmentService.GetByIdAsync(appointment.TreatmentId);
            return new AppointmentResponseDto
            {
                Id = appointment.Id,
                ClientId = appointment.ClientId,
                ClientName = clientResult.Data?.FullName,
                TreatmentId = appointment.TreatmentId,
                TreatmentName = treatmentResult.Data?.Name,
                AssignedStaff = appointment.AssignedStaff,
                AppointmentDateTime = appointment.AppointmentDateTime,
                DurationMinutes = appointment.DurationMinutes,
                Notes = appointment.Notes,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt
            };
        }

        public async Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetAllAsync()
        {
            var appointments = await _appointmentRepo.GetAllAsync();
            var dtos = new List<AppointmentResponseDto>();
            foreach (var apt in appointments)
                dtos.Add(await EnrichDto(apt));
            return ServiceResult<IEnumerable<AppointmentResponseDto>>.Success(dtos);
        }


        public async Task<ServiceResult<AppointmentResponseDto>> GetByIdAsync(int id)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(id);
            if (appointment == null)
                return ServiceResult<AppointmentResponseDto>.Failure("Appointment not found.");
            return ServiceResult<AppointmentResponseDto>.Success(await EnrichDto(appointment));
        }

        public async Task<ServiceResult<AppointmentResponseDto>> CreateAsync(CreateAppointmentDto dto)
        {
            var treatmentResult = await _treatmentService.GetByIdAsync(dto.TreatmentId);
            if (!treatmentResult.IsSuccess)
                return ServiceResult<AppointmentResponseDto>.Failure("Treatment not found.");

            var appointment = new Appointment
            {
                ClientId = dto.ClientId,
                TreatmentId = dto.TreatmentId,
                AssignedStaff = dto.AssignedStaff,
                AppointmentDateTime = dto.AppointmentDateTime,
                Notes = dto.Notes,
                Status = AppointmentStatus.Scheduled,
                DurationMinutes = treatmentResult.Data!.DurationMinutes
            };
            var created = await _appointmentRepo.AddAsync(appointment);
            return ServiceResult<AppointmentResponseDto>.Success(await EnrichDto(created));
        }

        public async Task<ServiceResult<AppointmentResponseDto>> UpdateAsync(int id, UpdateAppointmentDto dto)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(id);
            if (appointment == null)
                return ServiceResult<AppointmentResponseDto>.Failure("Appointment not found.");

            if (dto.ClientId.HasValue) appointment.ClientId = dto.ClientId.Value;
            if (dto.TreatmentId.HasValue) appointment.TreatmentId = dto.TreatmentId.Value;
            if (dto.AssignedStaff != null) appointment.AssignedStaff = dto.AssignedStaff;
            if (dto.AppointmentDateTime.HasValue) appointment.AppointmentDateTime = dto.AppointmentDateTime.Value;
            if (dto.Notes != null) appointment.Notes = dto.Notes;

            await _appointmentRepo.UpdateAsync(appointment);
            return ServiceResult<AppointmentResponseDto>.Success(await EnrichDto(appointment));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(id);
            if (appointment == null)
                return ServiceResult<bool>.Failure("Appointment not found.");
            await _appointmentRepo.DeleteAsync(appointment);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> UpdateStatusAsync(int id, string newStatus)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(id);
            if (appointment == null)
                return ServiceResult<bool>.Failure("Appointment not found.");
            var oldStatus = appointment.Status;
            appointment.Status = newStatus;
            await _appointmentRepo.UpdateAsync(appointment);

            await _stateTransition.OnStatusChangedAsync(appointment, oldStatus, newStatus);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetByClientAsync(int clientId)
        {
            var appointments = await _appointmentRepo.GetByClientAsync(clientId);
            var dtos = new List<AppointmentResponseDto>();
            foreach (var apt in appointments)
                dtos.Add(await EnrichDto(apt));
            return ServiceResult<IEnumerable<AppointmentResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            var appointments = await _appointmentRepo.GetByDateRangeAsync(start, end);
            var dtos = new List<AppointmentResponseDto>();
            foreach (var apt in appointments)
                dtos.Add(await EnrichDto(apt));
            return ServiceResult<IEnumerable<AppointmentResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<bool>> CheckAvailabilityAsync(int staffId, DateTime startTime, int durationMinutes)
        {
            var available = await _appointmentRepo.IsTimeSlotAvailableAsync(staffId, startTime, durationMinutes);
            return ServiceResult<bool>.Success(available);
        }

        public async Task<ServiceResult<PaginatedResult<AppointmentResponseDto>>> GetPaginatedAsync(
        int page,
        int pageSize,
        int? clientId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
        {
            // Bumuo ng filter expression
            Expression<Func<Appointment, bool>>? filter = null;
            if (clientId.HasValue || !string.IsNullOrEmpty(status) || fromDate.HasValue || toDate.HasValue)
            {
                filter = a =>
                    (!clientId.HasValue || a.ClientId == clientId.Value) &&
                    (string.IsNullOrEmpty(status) || a.Status == status) &&
                    (!fromDate.HasValue || a.AppointmentDateTime >= fromDate.Value) &&
                    (!toDate.HasValue || a.AppointmentDateTime <= toDate.Value);
            }

            var paginated = await _appointmentRepo.GetPaginatedWithDetailsAsync(page, pageSize, filter);
            var dtos = new List<AppointmentResponseDto>();
            foreach (var appointment in paginated.Items)
            {
                dtos.Add(await EnrichDto(appointment));
            }

            var result = new PaginatedResult<AppointmentResponseDto>
            {
                Items = dtos,
                Page = paginated.Page,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount
            };
            return ServiceResult<PaginatedResult<AppointmentResponseDto>>.Success(result);
        }
    }
}