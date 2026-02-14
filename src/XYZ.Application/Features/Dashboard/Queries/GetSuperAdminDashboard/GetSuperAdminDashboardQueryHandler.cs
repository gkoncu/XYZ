using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard;

public sealed class GetSuperAdminDashboardQueryHandler
    : IRequestHandler<GetSuperAdminDashboardQuery, SuperAdminDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _current;

    public GetSuperAdminDashboardQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _current = currentUser;
    }

    public async Task<SuperAdminDashboardDto> Handle(
        GetSuperAdminDashboardQuery request,
        CancellationToken ct)
    {
        var utcNow = DateTime.UtcNow;
        var todayDateOnly = DateOnly.FromDateTime(utcNow);

        var today = utcNow.Date;
        var expiringEnd = today.AddDays(30);

        var tenantsQ = _context.Tenants.IgnoreQueryFilters();

        var totalTenants = await tenantsQ.CountAsync(ct);
        var activeTenants = await tenantsQ.CountAsync(t => t.IsActive, ct);

        var activeTenantId =
            _current.TenantId.HasValue && _current.TenantId.Value > 0
                ? _current.TenantId
                : null;

        var activeTenantName = string.Empty;

        if (activeTenantId.HasValue)
        {
            activeTenantName = await tenantsQ
                .Where(t => t.Id == activeTenantId.Value)
                .Select(t => t.Name)
                .FirstOrDefaultAsync(ct) ?? string.Empty;
        }

        var totalStudents = await _context.Students
            .IgnoreQueryFilters()
            .Where(s => s.IsActive)
            .CountAsync(ct);

        var totalCoaches = await _context.Coaches
            .IgnoreQueryFilters()
            .Where(c => c.IsActive)
            .CountAsync(ct);

        var totalClasses = await _context.Classes
            .IgnoreQueryFilters()
            .Where(c => c.IsActive)
            .CountAsync(ct);

        var upcomingSessions = await _context.ClassSessions
            .IgnoreQueryFilters()
            .Where(cs =>
                cs.IsActive
                && cs.Date >= todayDateOnly
                && cs.Status == SessionStatus.Scheduled)
            .CountAsync(ct);

        var expiringBaseQuery = tenantsQ
            .Where(t => t.IsActive && t.SubscriptionEndDate >= today && t.SubscriptionEndDate <= expiringEnd);

        var expiringTenantsIn30Days = await expiringBaseQuery.CountAsync(ct);

        var expiringTenants = await expiringBaseQuery
            .OrderBy(t => t.SubscriptionEndDate)
            .Select(t => new ExpiringTenantListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Subdomain = t.Subdomain,
                SubscriptionEndDate = t.SubscriptionEndDate,
                DaysRemaining = EF.Functions.DateDiffDay(today, t.SubscriptionEndDate),
                IsActive = t.IsActive
            })
            .Take(15)
            .ToListAsync(ct);

        return new SuperAdminDashboardDto
        {
            ActiveTenantId = activeTenantId,
            ActiveTenantName = activeTenantName,

            TotalTenants = totalTenants,
            ActiveTenants = activeTenants,
            TotalStudents = totalStudents,
            TotalCoaches = totalCoaches,
            TotalClasses = totalClasses,

            UpcomingSessions = upcomingSessions,

            ExpiringTenantsIn30Days = expiringTenantsIn30Days,
            ExpiringTenants = expiringTenants,

            SystemHealth = new SuperAdminSystemHealthDto
            {
                ServerUtcNow = utcNow
            }
        };
    }
}