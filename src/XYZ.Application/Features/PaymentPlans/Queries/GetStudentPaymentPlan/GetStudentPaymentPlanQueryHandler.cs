using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan
{
    public class GetStudentPaymentPlanQueryHandler
        : IRequestHandler<GetStudentPaymentPlanQuery, StudentPaymentPlanDto?>
    {
        private readonly IDataScopeService _dataScope;

        public GetStudentPaymentPlanQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<StudentPaymentPlanDto?> Handle(
            GetStudentPaymentPlanQuery request,
            CancellationToken ct)
        {
            var plan = await _dataScope.PaymentPlans()
                .Include(pp => pp.Student)
                    .ThenInclude(s => s.User)
                .Include(pp => pp.Payments)
                .Where(pp => pp.StudentId == request.StudentId)
                .OrderByDescending(pp => pp.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (plan == null)
            {
                return null;
            }

            var installments = plan.Payments
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

            var totalPaid = installments
                .Where(i => i.Status == PaymentStatus.Paid)
                .Sum(i => i.Amount - (i.DiscountAmount ?? 0m));

            var totalAmount = plan.TotalAmount;
            var totalRemaining = totalAmount - totalPaid;

            return new StudentPaymentPlanDto
            {
                PlanId = plan.Id,
                StudentId = plan.StudentId,
                StudentFullName = plan.Student.User.FullName,
                TotalAmount = plan.TotalAmount,
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
