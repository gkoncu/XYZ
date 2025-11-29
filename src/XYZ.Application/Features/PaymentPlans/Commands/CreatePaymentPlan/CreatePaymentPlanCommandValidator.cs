using FluentValidation;
using System;

namespace XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan
{
    public class CreatePaymentPlanCommandValidator
        : AbstractValidator<CreatePaymentPlanCommand>
    {
        public CreatePaymentPlanCommandValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0);

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0);

            RuleFor(x => x.TotalInstallments)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.FirstDueDate.Date)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date.AddDays(-1));
        }
    }
}
