using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetPaymentPlanDetails
{
    public class GetPaymentPlanDetailsQueryHandler : IRequestHandler<GetPaymentPlanDetailsQuery, StudentPaymentPlanDto?>
    {
        private readonly IDataScopeService _dataScope;

        public GetPaymentPlanDetailsQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<StudentPaymentPlanDto?> Handle(GetPaymentPlanDetailsQuery request, CancellationToken ct)
        {
            var plan = await _dataScope.PaymentPlans()
                .Include(pp => pp.Student).ThenInclude(s => s.User)
                .Include(pp => pp.Payments)
                .Where(pp => pp.Id == request.PlanId && pp.IsActive)
                .FirstOrDefaultAsync(ct);

            if (plan == null)
                return null;

            var installments = plan.Payments
                .Where(p => p.IsActive && p.Status != PaymentStatus.Cancelled)
                .OrderBy(p => p.InstallmentNumber)
                .ThenBy(p => p.DueDate)
                .Select(p => new StudentPaymentInstallmentDto
                {
                    PaymentId = p.Id,
                    InstallmentNumber = p.InstallmentNumber,
                    Amount = p.Amount,
                    DiscountAmount = p.DiscountAmount,
                    Status = p.Status,
                    DueDate = p.DueDate,
                    PaidDate = p.PaidDate
                })
                .ToList();

            decimal Net(StudentPaymentInstallmentDto i) => i.Amount - (i.DiscountAmount ?? 0m);

            var plannedTotal = installments.Sum(Net);
            var totalPaid = installments.Where(i => i.Status == PaymentStatus.Paid).Sum(Net);
            var totalRemaining = installments.Where(i => i.Status == PaymentStatus.Pending || i.Status == PaymentStatus.Overdue).Sum(Net);

            return new StudentPaymentPlanDto
            {
                PlanId = plan.Id,
                StudentId = plan.StudentId,
                StudentFullName = plan.Student.User.FullName,
                TotalAmount = plannedTotal,
                TotalInstallments = plan.TotalInstallments,
                FirstDueDate = plan.FirstDueDate,
                IsInstallment = plan.IsInstallment,
                Status = plan.Status,
                TotalPaid = totalPaid,
                TotalRemaining = totalRemaining,
                LastUpdatedAt = plan.UpdatedAt,
                Installments = installments
            };
        }
    }
}
