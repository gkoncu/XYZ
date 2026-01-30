using MediatR;

namespace XYZ.Application.Features.PaymentPlans.Commands.CancelPaymentPlan
{
    public class CancelPaymentPlanCommand : IRequest<int>
    {
        public int PlanId { get; set; }
    }
}
