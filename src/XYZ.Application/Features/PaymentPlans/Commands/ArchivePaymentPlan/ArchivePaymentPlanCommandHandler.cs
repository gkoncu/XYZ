using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Commands.ArchivePaymentPlan
{
    public class ArchivePaymentPlanCommandHandler : IRequestHandler<ArchivePaymentPlanCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public ArchivePaymentPlanCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(ArchivePaymentPlanCommand request, CancellationToken ct)
        {
            var plan = await _dataScope.PaymentPlans()
                .Include(pp => pp.Payments)
                .FirstOrDefaultAsync(pp => pp.Id == request.PlanId && pp.IsActive, ct);

            if (plan == null)
                throw new InvalidOperationException("Aidat planı bulunamadı.");

            if (plan.Status != PaymentPlanStatus.Active)
                throw new InvalidOperationException("Sadece aktif plan arşivlenebilir.");

            var hasPendingOrOverdue = plan.Payments.Any(p =>
                p.IsActive && (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Overdue));

            if (hasPendingOrOverdue)
                throw new InvalidOperationException("Beklemede/gecikmiş ödeme varken plan arşivlenemez.");

            plan.Status = PaymentPlanStatus.Archived;

            await _context.SaveChangesAsync(ct);

            return plan.Id;
        }
    }
}
