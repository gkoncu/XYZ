using MediatR;
using System.Collections.Generic;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlanHistory
{
    public class GetStudentPaymentPlanHistoryQuery : IRequest<IList<PaymentPlanHistoryItemDto>>
    {
        public int StudentId { get; set; }
    }
}
