using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard
{
    public class GetSuperAdminDashboardQueryHandler
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
            if (_current.Role != "SuperAdmin")
                throw new UnauthorizedAccessException("Bu dashboard sadece SuperAdmin içindir.");

            var utcNow = DateTime.UtcNow;
            var todayDateOnly = DateOnly.FromDateTime(utcNow);

            var today = utcNow.Date;
            var expiringEnd = today.AddDays(30);

            var totalTenants = await _context.Tenants.CountAsync(ct);
            var activeTenants = await _context.Tenants.CountAsync(t => t.IsActive, ct);

            int? activeTenantId = _current.TenantId > 0 ? _current.TenantId : null;
            string activeTenantName = string.Empty;

            if (activeTenantId.HasValue)
            {
                activeTenantName = await _context.Tenants
                    .Where(t => t.Id == activeTenantId.Value)
                    .Select(t => t.Name)
                    .FirstOrDefaultAsync(ct) ?? string.Empty;
            }

            var totalStudents = await _context.Students.CountAsync(ct);
            var totalCoaches = await _context.Coaches.CountAsync(ct);
            var totalClasses = await _context.Classes.CountAsync(ct);

            var totalPaidAmount = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => p.Amount - (p.DiscountAmount ?? 0m), ct);

            var upcomingSessions = await _context.ClassSessions
                .CountAsync(cs =>
                    cs.Date >= todayDateOnly &&
                    cs.Status == SessionStatus.Scheduled &&
                    cs.IsActive,
                    ct);

            var expiringBaseQuery = _context.Tenants
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
}
