using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Commands.CancelPaymentPlan
{
    public class CancelPaymentPlanCommandHandler : IRequestHandler<CancelPaymentPlanCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public CancelPaymentPlanCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
        }

        public async Task<int> Handle(CancelPaymentPlanCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Ödeme planı iptal etme yetkiniz yok.");

            var plan = await _dataScope.PaymentPlans()
                .Include(pp => pp.Payments)
                .FirstOrDefaultAsync(pp => pp.Id == request.PlanId && pp.IsActive, ct);

            if (plan == null)
                throw new NotFoundException("PaymentPlan", request.PlanId);

            if (plan.Status == PaymentPlanStatus.Cancelled)
                return plan.Id;

            var now = DateTime.UtcNow;

            plan.Status = PaymentPlanStatus.Cancelled;
            plan.UpdatedAt = now;

            foreach (var p in plan.Payments.Where(x => x.IsActive && x.Status != PaymentStatus.Paid && x.Status != PaymentStatus.Cancelled))
            {
                p.Status = PaymentStatus.Cancelled;
                p.UpdatedAt = now;
            }

            await _context.SaveChangesAsync(ct);
            return plan.Id;
        }
    }
}
