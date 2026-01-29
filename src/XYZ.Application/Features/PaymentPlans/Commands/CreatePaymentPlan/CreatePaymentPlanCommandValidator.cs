using FluentValidation;
using System;

namespace XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan
{
    public class CreatePaymentPlanCommandValidator : AbstractValidator<CreatePaymentPlanCommand>
    {
        public CreatePaymentPlanCommandValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0);

            RuleFor(x => x.TotalAmount)
                .GreaterThan(1)
                .LessThan(99999);

            RuleFor(x => x.FirstDueDate.Date)
                .Must(d =>
                {
                    var today = DateTime.UtcNow.Date;
                    return d >= today.AddDays(-365) && d <= today.AddDays(365);
                })
                .WithMessage("FirstDueDate must be within 1 year (past/future).");

            RuleFor(x => x)
                .Must(x => !x.IsInstallment ? x.TotalInstallments == 1 : x.TotalInstallments >= 2)
                .WithMessage("TotalInstallments is invalid for the selected plan type.");

            RuleFor(x => x.TotalInstallments)
                .InclusiveBetween(1, 52)
                .WithMessage("TotalInstallments must be between 1 and 52.");
        }
    }
}
