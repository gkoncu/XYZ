using MediatR;
using Microsoft.EntityFrameworkCore;
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
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Bu dashboard sadece Admin, Coach ve SuperAdmin rollerine açıktır.");

            var today = DateOnly.FromDateTime(DateTime.Today);

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

            var payments = _dataScope.Payments();

            var pendingPaymentsQuery = payments
                .Where(p => p.Status == PaymentStatus.Pending);

            var overduePaymentsQuery = payments
                .Where(p => p.Status == PaymentStatus.Overdue);

            var pendingPaymentsCount = await pendingPaymentsQuery.CountAsync(ct);
            var pendingPaymentsAmount = await pendingPaymentsQuery
                .SumAsync(p => p.Amount - (p.DiscountAmount ?? 0m), ct);

            var overduePaymentsCount = await overduePaymentsQuery.CountAsync(ct);
            var overduePaymentsAmount = await overduePaymentsQuery
                .SumAsync(p => p.Amount - (p.DiscountAmount ?? 0m), ct);

            var dueStart = DateTime.Today;
            var dueEnd = DateTime.Today.AddDays(7);

            var upcomingDuePaymentsCount = await payments
                .Where(p => p.DueDate >= dueStart && p.DueDate <= dueEnd)
                .CountAsync(ct);

            var now = DateTime.UtcNow;
            var activeAnnouncementsCount = await _dataScope.Announcements()
                .Where(a => a.PublishDate <= now && (a.ExpiryDate == null || a.ExpiryDate >= now))
                .CountAsync(ct);

            var tenantId = _current.TenantId;

            var incompleteStudentDocumentsCount = 0;
            var incompleteCoachDocumentsCount = 0;

            if (tenantId.HasValue)
            {
                incompleteStudentDocumentsCount = await GetIncompleteOwnerCount(
                    target: DocumentTarget.Student,
                    tenantId: tenantId.Value,
                    ct);

                if (role == "Admin" || role == "SuperAdmin")
                {
                    incompleteCoachDocumentsCount = await GetIncompleteOwnerCount(
                        target: DocumentTarget.Coach,
                        tenantId: tenantId.Value,
                        ct);
                }
            }

            return new AdminCoachDashboardDto
            {
                StudentCount = studentCount,
                ClassCount = classCount,
                TodaySessionsCount = todaySessionsCount,
                UpcomingSessionsCount = upcomingSessionsCount,

                PendingPaymentsCount = pendingPaymentsCount,
                PendingPaymentsAmount = pendingPaymentsAmount,
                OverduePaymentsCount = overduePaymentsCount,
                OverduePaymentsAmount = overduePaymentsAmount,
                UpcomingDuePaymentsCount = upcomingDuePaymentsCount,

                ActiveAnnouncementsCount = activeAnnouncementsCount,

                IncompleteStudentDocumentsCount = incompleteStudentDocumentsCount,
                IncompleteCoachDocumentsCount = incompleteCoachDocumentsCount
            };
        }

        private async Task<int> GetIncompleteOwnerCount(
            DocumentTarget target,
            int tenantId,
            CancellationToken ct)
        {
            var requiredDefinitionIds = await _context.DocumentDefinitions
                .Where(d => d.TenantId == tenantId
                            && d.Target == target
                            && d.IsRequired)
                .Select(d => d.Id)
                .ToListAsync(ct);

            if (requiredDefinitionIds.Count == 0)
                return 0;

            if (target == DocumentTarget.Student)
            {
                var studentIds = await _dataScope.Students()
                    .Select(s => s.Id)
                    .ToListAsync(ct);

                if (studentIds.Count == 0)
                    return 0;

                var uploadedCounts = await _dataScope.Documents()
                    .Where(d => d.StudentId != null
                                && studentIds.Contains(d.StudentId.Value)
                                && requiredDefinitionIds.Contains(d.DocumentDefinitionId))
                    .Select(d => new { OwnerId = d.StudentId!.Value, d.DocumentDefinitionId })
                    .Distinct()
                    .GroupBy(x => x.OwnerId)
                    .Select(g => new { OwnerId = g.Key, Count = g.Count() })
                    .ToListAsync(ct);

                var map = uploadedCounts.ToDictionary(x => x.OwnerId, x => x.Count);
                var requiredCount = requiredDefinitionIds.Count;
                return studentIds.Count(id => !map.TryGetValue(id, out var c) || c < requiredCount);
            }

            var coachIds = await _dataScope.Coaches()
                .Select(c => c.Id)
                .ToListAsync(ct);

            if (coachIds.Count == 0)
                return 0;

            var uploadedCountsCoach = await _dataScope.Documents()
                .Where(d => d.CoachId != null
                            && coachIds.Contains(d.CoachId.Value)
                            && requiredDefinitionIds.Contains(d.DocumentDefinitionId))
                .Select(d => new { OwnerId = d.CoachId!.Value, d.DocumentDefinitionId })
                .Distinct()
                .GroupBy(x => x.OwnerId)
                .Select(g => new { OwnerId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var mapCoach = uploadedCountsCoach.ToDictionary(x => x.OwnerId, x => x.Count);
            var requiredCountCoach = requiredDefinitionIds.Count;
            return coachIds.Count(id => !mapCoach.TryGetValue(id, out var c) || c < requiredCountCoach);
        }
    }
}
