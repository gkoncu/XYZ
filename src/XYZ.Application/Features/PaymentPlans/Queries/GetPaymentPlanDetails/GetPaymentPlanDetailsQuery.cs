using MediatR;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetPaymentPlanDetails
{
    public class GetPaymentPlanDetailsQuery : IRequest<StudentPaymentPlanDto?>
    {
        public int PlanId { get; set; }
    }
}
