using FluentValidation;

namespace XYZ.Application.Features.PaymentPlans.Commands.ArchivePaymentPlan
{
    public class ArchivePaymentPlanCommandValidator : AbstractValidator<ArchivePaymentPlanCommand>
    {
        public ArchivePaymentPlanCommandValidator()
        {
            RuleFor(x => x.PlanId)
                .GreaterThan(0);
        }
    }
}
