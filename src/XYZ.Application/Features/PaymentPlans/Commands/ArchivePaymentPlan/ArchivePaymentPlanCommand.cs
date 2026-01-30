using MediatR;

namespace XYZ.Application.Features.PaymentPlans.Commands.ArchivePaymentPlan
{
    public class ArchivePaymentPlanCommand : IRequest<int>
    {
        public int PlanId { get; set; }
    }
}
