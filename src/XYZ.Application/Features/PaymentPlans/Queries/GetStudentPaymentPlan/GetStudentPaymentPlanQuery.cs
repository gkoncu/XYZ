using MediatR;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan
{
    public class GetStudentPaymentPlanQuery : IRequest<StudentPaymentPlanDto?>
    {
        public int StudentId { get; set; }
    }
}
