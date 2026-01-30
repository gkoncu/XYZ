using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan
{
    public class CreatePaymentPlanCommandHandler : IRequestHandler<CreatePaymentPlanCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public CreatePaymentPlanCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
        }

        public async Task<int> Handle(CreatePaymentPlanCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Ödeme planı oluşturma yetkiniz yok.");

            var student = await _dataScope.Students()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student == null)
                throw new NotFoundException(nameof(Student), request.StudentId);

            var tenantId = student.TenantId;
            var now = DateTime.UtcNow;

            var activePlan = await _dataScope.PaymentPlans()
                .Include(pp => pp.Payments)
                .Where(pp => pp.StudentId == student.Id && pp.Status == PaymentPlanStatus.Active && pp.IsActive)
                .OrderByDescending(pp => pp.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (activePlan != null)
            {
                activePlan.Status = PaymentPlanStatus.Archived;
                activePlan.UpdatedAt = now;

                foreach (var p in activePlan.Payments.Where(x => x.IsActive && x.Status != PaymentStatus.Paid && x.Status != PaymentStatus.Cancelled))
                {
                    p.Status = PaymentStatus.Cancelled;
                    p.UpdatedAt = now;
                }
            }

            var totalInstallments =
                request.IsInstallment
                    ? Math.Max(1, request.TotalInstallments)
                    : 1;

            if (!request.IsInstallment)
                totalInstallments = 1;

            var plan = new PaymentPlan
            {
                StudentId = student.Id,
                TenantId = tenantId,
                TotalAmount = request.TotalAmount,
                TotalInstallments = totalInstallments,
                FirstDueDate = request.FirstDueDate.Date,
                IsInstallment = totalInstallments > 1,
                Name = request.IsInstallment ? $"{totalInstallments} Taksit Planı" : "Peşin Ödeme",
                Status = PaymentPlanStatus.Active,
                CreatedAt = now,
                IsActive = true,
                Notes = null
            };

            await _context.PaymentPlans.AddAsync(plan, ct);

            var baseInstallmentAmount =
                Math.Floor((request.TotalAmount / totalInstallments) * 100) / 100m;

            var payments = Enumerable.Range(1, totalInstallments)
                .Select(i =>
                {
                    var amount = baseInstallmentAmount;
                    if (i == totalInstallments)
                    {
                        var subtotal = baseInstallmentAmount * (totalInstallments - 1);
                        amount = request.TotalAmount - subtotal;
                    }

                    var dueDate = request.IsInstallment
                        ? request.FirstDueDate.Date.AddMonths(i - 1)
                        : request.FirstDueDate.Date;

                    return new Payment
                    {
                        StudentId = student.Id,
                        TenantId = tenantId,
                        Amount = amount,
                        DiscountAmount = null,
                        DiscountReason = null,
                        Status = PaymentStatus.Pending,
                        DueDate = dueDate,
                        PaidDate = null,
                        PaymentMethod = null,
                        TransactionId = null,
                        Notes = null,
                        InstallmentNumber = i,
                        TotalInstallments = totalInstallments,
                        IsInstallment = totalInstallments > 1,
                        InstallmentPlan = plan.Name,
                        PaymentPlan = plan,
                        CreatedAt = now,
                        IsActive = true
                    };
                })
                .ToList();

            await _context.Payments.AddRangeAsync(payments, ct);
            await _context.SaveChangesAsync(ct);

            return plan.Id;
        }
    }
}
