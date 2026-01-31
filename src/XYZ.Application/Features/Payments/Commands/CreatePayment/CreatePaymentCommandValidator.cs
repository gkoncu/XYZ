using FluentValidation;
using System;

namespace XYZ.Application.Features.Payments.Commands.CreatePayment
{
    public class CreatePaymentCommandValidator
        : AbstractValidator<CreatePaymentCommand>
    {
        public CreatePaymentCommandValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .LessThanOrEqualTo(99999);

            RuleFor(x => x.DueDate)
                .Must(d =>
                {
                    var today = DateTime.UtcNow.Date;
                    var date = d.Date;
                    return date >= today.AddDays(-365) && date <= today.AddDays(365);
                })
                .WithMessage("DueDate must be within 1 year (past/future).");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.DiscountAmount.HasValue);

            RuleFor(x => x)
                .Must(x => !x.DiscountAmount.HasValue || x.DiscountAmount.Value <= x.Amount)
                .WithMessage("DiscountAmount cannot be greater than Amount.");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));

            RuleFor(x => x.Status)
                .IsInEnum();
        }
    }
}
