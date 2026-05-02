using FluentValidation;
using AestheticClinicAPI.Modules.Billing.DTOs;

namespace AestheticClinicAPI.Modules.Billing.Validators;

public class UpdateInvoiceStatusValidator : AbstractValidator<UpdateInvoiceStatusDto>
{
    public UpdateInvoiceStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => new[] { "Draft", "Sent", "Paid", "Partial", "Overdue", "Cancelled" }.Contains(status))
            .WithMessage("Invalid status. Allowed values: Draft, Sent, Paid, Partial, Overdue, Cancelled.");
    }
}