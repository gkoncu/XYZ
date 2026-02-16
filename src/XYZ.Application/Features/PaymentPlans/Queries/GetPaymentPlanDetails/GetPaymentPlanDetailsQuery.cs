using MediatR;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetPaymentPlanDetails
{
    public class GetPaymentPlanDetailsQuery : IRequest<StudentPaymentPlanDto?>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Students.PaymentsRead;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int PlanId { get; set; }
    }
}
