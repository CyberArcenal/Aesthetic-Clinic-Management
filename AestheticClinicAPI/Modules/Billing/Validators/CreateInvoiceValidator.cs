using FluentValidation;
using AestheticClinicAPI.Modules.Billing.DTOs;

namespace AestheticClinicAPI.Modules.Billing.Validators;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0).WithMessage("Client ID is required.");

        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Issue date is required.");

        RuleFor(x => x.Subtotal)
            .GreaterThanOrEqualTo(0).WithMessage("Subtotal must be zero or positive.");

        RuleFor(x => x.Tax)
            .GreaterThanOrEqualTo(0).WithMessage("Tax must be zero or positive.");
    }
}