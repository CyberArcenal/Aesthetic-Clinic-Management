using FluentValidation;
using AestheticClinicAPI.Modules.Appointments.DTOs;

namespace AestheticClinicAPI.Modules.Appointments.Validators;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentDto>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0).WithMessage("Client ID must be a positive number.");

        RuleFor(x => x.TreatmentId)
            .GreaterThan(0).WithMessage("Treatment ID must be a positive number.");

        RuleFor(x => x.AppointmentDateTime)
            .NotEmpty().WithMessage("Appointment date/time is required.")
            .Must(BeAFutureDate).WithMessage("Appointment must be scheduled in the future.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
    }

    private static bool BeAFutureDate(DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }
}