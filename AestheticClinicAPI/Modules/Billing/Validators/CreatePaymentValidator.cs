using FluentValidation;
using AestheticClinicAPI.Modules.Billing.DTOs;

namespace AestheticClinicAPI.Modules.Billing.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("Invoice ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required.");

        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("Payment method is required.")
            .Must(method => new[] { "Cash", "CreditCard", "DebitCard", "GCash", "BankTransfer" }.Contains(method))
            .WithMessage("Invalid payment method.");
    }
}