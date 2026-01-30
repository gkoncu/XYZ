using FluentValidation;

namespace XYZ.Application.Features.PaymentPlans.Commands.CancelPaymentPlan
{
    public class CancelPaymentPlanCommandValidator : AbstractValidator<CancelPaymentPlanCommand>
    {
        public CancelPaymentPlanCommandValidator()
        {
            RuleFor(x => x.PlanId)
                .GreaterThan(0);
        }
    }
}
