using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard
{
    public class GetAdminCoachDashboardQueryHandler
        : IRequestHandler<GetAdminCoachDashboardQuery, AdminCoachDashboardDto>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public GetAdminCoachDashboardQueryHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _dataScope = dataScope;
            _context = context;
            _current = currentUser;
        }

        public async Task<AdminCoachDashboardDto> Handle(
            GetAdminCoachDashboardQuery request,
            CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach"))
                throw new UnauthorizedAccessException("Bu dashboard sadece Admin ve Coach rollerine açıktır.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var studentCount = await _dataScope.Students().CountAsync(ct);
            var classCount = await _dataScope.Classes().CountAsync(ct);

            var scopedClasses = _dataScope.Classes();

            var sessionsQuery = _context.ClassSessions
                .Where(cs => cs.IsActive)
                .Join(scopedClasses,
                    cs => cs.ClassId,
                    c => c.Id,
                    (cs, c) => cs);

            var todaySessionsCount = await sessionsQuery
                .Where(cs => cs.Date == today && cs.Status == SessionStatus.Scheduled)
                .CountAsync(ct);

            var upcomingSessionsCount = await sessionsQuery
                .Where(cs => cs.Date >= today && cs.Status == SessionStatus.Scheduled)
                .CountAsync(ct);

            var pendingPaymentsQuery = _dataScope.Payments()
                .Where(p => p.Status == PaymentStatus.Pending
                            || p.Status == PaymentStatus.Overdue);

            var pendingPaymentsCount = await pendingPaymentsQuery.CountAsync(ct);
            var pendingPaymentsAmount = await pendingPaymentsQuery
                .SumAsync(p => p.Amount - (p.DiscountAmount ?? 0m), ct);

            var lastWeek = DateTime.UtcNow.AddDays(-7);

            var recentAnnouncementsCount = await _dataScope.Announcements()
                .Where(a => a.PublishDate >= lastWeek)
                .CountAsync(ct);

            return new AdminCoachDashboardDto
            {
                StudentCount = studentCount,
                ClassCount = classCount,
                TodaySessionsCount = todaySessionsCount,
                UpcomingSessionsCount = upcomingSessionsCount,
                PendingPaymentsCount = pendingPaymentsCount,
                PendingPaymentsAmount = pendingPaymentsAmount,
                RecentAnnouncementsCount = recentAnnouncementsCount
            };
        }
    }
}
