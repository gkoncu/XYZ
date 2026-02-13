using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Data;
using XYZ.Domain.Enums;

namespace XYZ.Application.Services;

public sealed class OverduePaymentService(IApplicationDbContext context) : IOverduePaymentService
{
    private readonly IApplicationDbContext _context = context;

    public Task<int> MarkOverdueAsync(CancellationToken ct = default)
        => MarkOverdueAsync(DateTime.Today, ct);

    public async Task<int> MarkOverdueAsync(DateTime todayLocal, CancellationToken ct = default)
    {

        var nowUtc = DateTime.UtcNow;

        var affected = await _context.Payments.IgnoreQueryFilters().Where(p => p.IsActive == true)
            .Where(p => p.Status == PaymentStatus.Pending && p.DueDate < todayLocal)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Status, PaymentStatus.Overdue)
                .SetProperty(p => p.UpdatedAt, nowUtc),
                ct);

        return affected;
    }
}
