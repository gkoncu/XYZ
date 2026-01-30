using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Commands.ArchivePaymentPlan
{
    public class ArchivePaymentPlanCommandHandler : IRequestHandler<ArchivePaymentPlanCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public ArchivePaymentPlanCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
        }

        public async Task<int> Handle(ArchivePaymentPlanCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Ödeme planı arşivleme yetkiniz yok.");

            var plan = await _dataScope.PaymentPlans()
                .Include(pp => pp.Payments)
                .FirstOrDefaultAsync(pp => pp.Id == request.PlanId && pp.IsActive, ct);

            if (plan == null)
                throw new NotFoundException("PaymentPlan", request.PlanId);

            if (plan.Status == PaymentPlanStatus.Archived)
                return plan.Id;

            var hasOpen = plan.Payments.Any(p => p.IsActive && p.Status != PaymentStatus.Paid && p.Status != PaymentStatus.Cancelled);
            if (hasOpen)
                throw new InvalidOperationException("Bu planın arşivlenebilmesi için tüm taksitlerin Ödendi veya İptal durumunda olması gerekir.");

            var now = DateTime.UtcNow;
            plan.Status = PaymentPlanStatus.Archived;
            plan.UpdatedAt = now;

            await _context.SaveChangesAsync(ct);
            return plan.Id;
        }
    }
}
