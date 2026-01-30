using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlanHistory
{
    public class GetStudentPaymentPlanHistoryQueryHandler
        : IRequestHandler<GetStudentPaymentPlanHistoryQuery, IList<PaymentPlanHistoryItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetStudentPaymentPlanHistoryQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<IList<PaymentPlanHistoryItemDto>> Handle(GetStudentPaymentPlanHistoryQuery request, CancellationToken ct)
        {
            var plans = await _dataScope.PaymentPlans()
                .Include(pp => pp.Payments)
                .Where(pp => pp.StudentId == request.StudentId && pp.IsActive)
                .OrderByDescending(pp => pp.CreatedAt)
                .ToListAsync(ct);

            decimal NetAmount(decimal amount, decimal? discount) => amount - (discount ?? 0m);

            return plans.Select(plan =>
            {
                var payments = plan.Payments
                    .Where(p => p.IsActive && p.Status != PaymentStatus.Cancelled)
                    .ToList();

                var total = payments.Sum(p => NetAmount(p.Amount, p.DiscountAmount));
                var paid = payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => NetAmount(p.Amount, p.DiscountAmount));
                var remaining = payments.Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Overdue)
                    .Sum(p => NetAmount(p.Amount, p.DiscountAmount));

                return new PaymentPlanHistoryItemDto
                {
                    PlanId = plan.Id,
                    CreatedAt = plan.CreatedAt,
                    PlanName = plan.Name ?? "",
                    Status = plan.Status,
                    TotalAmount = total,
                    TotalPaid = paid,
                    TotalRemaining = remaining,
                    TotalInstallments = plan.TotalInstallments,
                    FirstDueDate = plan.FirstDueDate,
                    IsInstallment = plan.IsInstallment
                };
            }).ToList();
        }
    }
}
