using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Commands.DeletePayment;

public sealed class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, int>
{
    private readonly IDataScopeService _dataScope;
    private readonly IApplicationDbContext _context;

    public DeletePaymentCommandHandler(IDataScopeService dataScope, IApplicationDbContext context)
    {
        _dataScope = dataScope;
        _context = context;
    }

    public async Task<int> Handle(DeletePaymentCommand request, CancellationToken ct)
    {
        var payment = await _dataScope.Payments()
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (payment is null)
            throw new NotFoundException("Payment", request.Id);

        var now = DateTime.UtcNow;

        if (payment.PaymentPlanId.HasValue)
        {
            if (payment.Status != PaymentStatus.Cancelled)
            {
                payment.Status = PaymentStatus.Cancelled;
                payment.PaidDate = null;
                payment.UpdatedAt = now;
            }

            await TryArchivePaymentPlanIfCompletedAsync(payment.PaymentPlanId.Value, now, ct);
            await _context.SaveChangesAsync(ct);
            return payment.Id;
        }

        payment.IsActive = false;
        payment.UpdatedAt = now;

        await _context.SaveChangesAsync(ct);
        return payment.Id;
    }

    private async Task TryArchivePaymentPlanIfCompletedAsync(int paymentPlanId, DateTime now, CancellationToken ct)
    {
        var plan = await _context.PaymentPlans
            .FirstOrDefaultAsync(x => x.Id == paymentPlanId, ct);

        if (plan is null)
            return;

        if (!plan.IsActive || plan.Status != PaymentPlanStatus.Active)
            return;

        var hasOpenInstallment = await _context.Payments
            .AnyAsync(p => p.PaymentPlanId == paymentPlanId
                          && p.IsActive
                          && p.Status != PaymentStatus.Paid
                          && p.Status != PaymentStatus.Cancelled,
                     ct);

        if (!hasOpenInstallment)
        {
            plan.Status = PaymentPlanStatus.Archived;
            plan.UpdatedAt = now;
        }
    }
}
