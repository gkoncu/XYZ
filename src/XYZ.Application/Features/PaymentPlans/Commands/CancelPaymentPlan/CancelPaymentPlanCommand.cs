using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Commands.CancelPaymentPlan
{
    public class CancelPaymentPlanCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Payments.UpdatePlan;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public int PlanId { get; set; }
    }
}
