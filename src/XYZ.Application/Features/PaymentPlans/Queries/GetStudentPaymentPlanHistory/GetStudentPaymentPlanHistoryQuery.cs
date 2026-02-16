using MediatR;
using System.Collections.Generic;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlanHistory
{
    public class GetStudentPaymentPlanHistoryQuery : IRequest<IList<PaymentPlanHistoryItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Students.PaymentsRead;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int StudentId { get; set; }
    }
}
