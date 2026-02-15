using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan
{
    public class CreatePaymentPlanCommandHandler : IRequestHandler<CreatePaymentPlanCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public CreatePaymentPlanCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(CreatePaymentPlanCommand request, CancellationToken ct)
        {
            var student = await _dataScope.Students()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student == null)
                throw new InvalidOperationException("Öğrenci bulunamadı.");

            var existingActivePlan = await _dataScope.PaymentPlans()
                .Where(pp => pp.StudentId == request.StudentId && pp.IsActive && pp.Status == PaymentPlanStatus.Active)
                .AnyAsync(ct);

            if (existingActivePlan)
                throw new InvalidOperationException("Bu öğrenci için zaten aktif bir aidat planı var.");

            var plan = new PaymentPlan
            {
                StudentId = request.StudentId,
                TotalAmount = request.TotalAmount,
                TotalInstallments = request.TotalInstallments,
                FirstDueDate = request.FirstDueDate.Date,
                IsInstallment = request.IsInstallment,
                Status = PaymentPlanStatus.Active,
                IsActive = true
            };

            _context.PaymentPlans.Add(plan);
            await _context.SaveChangesAsync(ct);

            if (request.TotalInstallments <= 0)
                throw new InvalidOperationException("Taksit sayısı 0 olamaz.");

            var installmentAmount = request.TotalAmount / request.TotalInstallments;

            for (int i = 1; i <= request.TotalInstallments; i++)
            {
                var dueDate = request.FirstDueDate.Date.AddMonths(i - 1);

                var payment = new Payment
                {
                    StudentId = request.StudentId,
                    PaymentPlanId = plan.Id,
                    InstallmentNumber = i,
                    Amount = decimal.Round(installmentAmount, 2),
                    DueDate = dueDate,
                    Status = PaymentStatus.Pending,
                    IsActive = true
                };

                _context.Payments.Add(payment);
            }

            await _context.SaveChangesAsync(ct);

            return plan.Id;
        }
    }
}
