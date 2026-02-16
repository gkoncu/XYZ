using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Commands.CancelPaymentPlan
{
    public class CancelPaymentPlanCommandHandler : IRequestHandler<CancelPaymentPlanCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public CancelPaymentPlanCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(CancelPaymentPlanCommand request, CancellationToken ct)
        {
            var plan = await _dataScope.PaymentPlans()
                .Include(pp => pp.Payments)
                .FirstOrDefaultAsync(pp => pp.Id == request.PlanId && pp.IsActive, ct);

            if (plan == null)
                throw new InvalidOperationException("Aidat planı bulunamadı.");

            if (plan.Status != PaymentPlanStatus.Active)
                throw new InvalidOperationException("Sadece aktif plan iptal edilebilir.");

            plan.Status = PaymentPlanStatus.Cancelled;

            foreach (var payment in plan.Payments.Where(p => p.IsActive && p.Status == PaymentStatus.Pending))
            {
                payment.Status = PaymentStatus.Cancelled;
            }

            await _context.SaveChangesAsync(ct);

            return plan.Id;
        }
    }
}
