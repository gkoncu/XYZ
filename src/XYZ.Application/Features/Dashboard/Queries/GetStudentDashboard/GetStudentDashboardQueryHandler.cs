using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard
{
    public sealed class GetStudentDashboardQueryHandler
        : IRequestHandler<GetStudentDashboardQuery, StudentDashboardDto>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public GetStudentDashboardQueryHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _dataScope = dataScope;
            _context = context;
            _current = currentUser;
        }

        public async Task<StudentDashboardDto> Handle(GetStudentDashboardQuery request, CancellationToken ct)
        {
            if (!_current.StudentId.HasValue)
                throw new UnauthorizedAccessException("Bu dashboard sadece öğrenci için kullanılabilir.");

            var studentId = _current.StudentId.Value;
            var tenantId = _current.TenantId;

            var nowUtc = DateTime.UtcNow;
            var todayLocal = DateTime.Today;
            var today = DateOnly.FromDateTime(todayLocal);

            // -----------------------------
            // 1) Upcoming sessions (next 7 days)
            // -----------------------------
            var end7 = today.AddDays(7);

            var upcomingSessionsQuery = _dataScope.Attendances()
                .AsNoTracking()
                .Where(a =>
                    a.StudentId == studentId &&
                    a.ClassSession.Status == SessionStatus.Scheduled &&
                    a.ClassSession.Date >= today &&
                    a.ClassSession.Date <= end7);

            var upcomingSessionsCount = await upcomingSessionsQuery.CountAsync(ct);

            var upcomingSessions = await upcomingSessionsQuery
                .OrderBy(a => a.ClassSession.Date)
                .ThenBy(a => a.ClassSession.StartTime)
                .Select(a => new UpcomingSessionListItemDto
                {
                    SessionId = a.ClassSessionId,
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    Date = a.ClassSession.Date,
                    StartTime = a.ClassSession.StartTime,
                    EndTime = a.ClassSession.EndTime,
                    Title = a.ClassSession.Title,
                    Location = a.ClassSession.Location,

                    CoachName = a.Class.Coaches
                        .Select(c => (c.User.FirstName + " " + c.User.LastName).Trim())
                        .FirstOrDefault()
                })
                .Take(10)
                .ToListAsync(ct);

            // -----------------------------
            // 2) Missing documents (only if missing)
            // -----------------------------
            var missingDocuments = new List<MissingDocumentListItemDto>();

            if (tenantId.HasValue)
            {
                var requiredDefs = await _context.DocumentDefinitions
                    .AsNoTracking()
                    .Where(d => d.TenantId == tenantId.Value &&
                                d.Target == DocumentTarget.Student &&
                                d.IsRequired)
                    .Select(d => new { d.Id, d.Name })
                    .ToListAsync(ct);

                if (requiredDefs.Count > 0)
                {
                    var requiredIds = requiredDefs.Select(x => x.Id).ToList();

                    var uploadedDefIds = await _dataScope.Documents()
                        .AsNoTracking()
                        .Where(d => d.StudentId == studentId && requiredIds.Contains(d.DocumentDefinitionId))
                        .Select(d => d.DocumentDefinitionId)
                        .Distinct()
                        .ToListAsync(ct);

                    var uploadedSet = uploadedDefIds.ToHashSet();

                    missingDocuments = requiredDefs
                        .Where(x => !uploadedSet.Contains(x.Id))
                        .Select(x => new MissingDocumentListItemDto
                        {
                            DocumentDefinitionId = x.Id,
                            Name = x.Name
                        })
                        .Take(5)
                        .ToList();
                }
            }

            var missingDocumentsCount = missingDocuments.Count;

            // -----------------------------
            // 3) Fees (Aidat) - Overdue always shown if exists
            // -----------------------------
            var overdueQuery = _dataScope.Payments()
                .AsNoTracking()
                .Where(p => p.StudentId == studentId && p.Status == PaymentStatus.Overdue);

            var overdueFeesCount = await overdueQuery.CountAsync(ct);
            var overdueFeesAmount = overdueFeesCount == 0
                ? 0m
                : await overdueQuery.SumAsync(p => p.Amount - (p.DiscountAmount ?? 0m), ct);

            var overdueFeesTop = overdueFeesCount == 0
                ? new List<FeeListItemDto>()
                : await overdueQuery
                    .OrderBy(p => p.DueDate)
                    .Take(3)
                    .Select(p => new FeeListItemDto
                    {
                        PaymentId = p.Id,
                        DueDate = p.DueDate,
                        Amount = p.Amount - (p.DiscountAmount ?? 0m),
                        Status = "Overdue"
                    })
                    .ToListAsync(ct);

            // -----------------------------
            // 4) Fees (Aidat) - Due soon only if within 3 days (Pending)
            // -----------------------------
            var dueSoonStart = todayLocal.Date;
            var dueSoonEnd = todayLocal.Date.AddDays(3);

            var dueSoonQuery = _dataScope.Payments()
                .AsNoTracking()
                .Where(p =>
                    p.StudentId == studentId &&
                    p.Status == PaymentStatus.Pending &&
                    p.DueDate >= dueSoonStart &&
                    p.DueDate <= dueSoonEnd);

            var dueSoonFeesCount = await dueSoonQuery.CountAsync(ct);
            var dueSoonFeesAmount = dueSoonFeesCount == 0
                ? 0m
                : await dueSoonQuery.SumAsync(p => p.Amount - (p.DiscountAmount ?? 0m), ct);

            var dueSoonFeesTop = dueSoonFeesCount == 0
                ? new List<FeeListItemDto>()
                : await dueSoonQuery
                    .OrderBy(p => p.DueDate)
                    .Take(3)
                    .Select(p => new FeeListItemDto
                    {
                        PaymentId = p.Id,
                        DueDate = p.DueDate,
                        Amount = p.Amount - (p.DiscountAmount ?? 0m),
                        Status = "Pending"
                    })
                    .ToListAsync(ct);

            // -----------------------------
            // 5) Announcements (Top 5, active)
            // -----------------------------
            var recentAnnouncements = await _dataScope.Announcements()
                .AsNoTracking()
                .Where(a => a.PublishDate <= nowUtc && (a.ExpiryDate == null || a.ExpiryDate >= nowUtc))
                .OrderByDescending(a => a.PublishDate)
                .Select(a => new RecentAnnouncementListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    PublishDate = a.PublishDate
                })
                .Take(5)
                .ToListAsync(ct);

            // -----------------------------
            // 6) Attendance overall + weekly trend (last 12 weeks)
            //     Percent denominator: PresentLike / (PresentLike + Absent), Excused excluded.
            // -----------------------------
            var weekCount = 12;
            var trendStart = StartOfWeek(today.AddDays(-(7 * (weekCount - 1))));

            var attendanceRows = await _dataScope.Attendances()
                .AsNoTracking()
                .Where(a => a.StudentId == studentId && a.ClassSession.Date >= trendStart)
                .Select(a => new { a.ClassSession.Date, a.Status })
                .ToListAsync(ct);

            var overallQuery = _dataScope.Attendances().AsNoTracking().Where(a => a.StudentId == studentId);

            var overallPresent = await overallQuery.CountAsync(a => a.Status == AttendanceStatus.Present, ct);
            var overallLate = await overallQuery.CountAsync(a => a.Status == AttendanceStatus.Late, ct);
            var overallAbsent = await overallQuery.CountAsync(a => a.Status == AttendanceStatus.Absent, ct);
            var overallExcused = await overallQuery.CountAsync(a => a.Status == AttendanceStatus.Excused, ct);
            var overallUnknown = await overallQuery.CountAsync(a => a.Status == AttendanceStatus.Unknown, ct);

            var presentLike = overallPresent + overallLate;
            var denom = presentLike + overallAbsent;
            var overallPercent = denom > 0 ? (int)Math.Round((presentLike * 100.0) / denom) : 0;

            var weeklyMap = new Dictionary<DateOnly, (int presentLike, int absent)>();

            foreach (var row in attendanceRows)
            {
                var ws = StartOfWeek(row.Date);

                if (!weeklyMap.TryGetValue(ws, out var acc))
                    acc = (0, 0);

                if (row.Status == AttendanceStatus.Present || row.Status == AttendanceStatus.Late)
                    acc.presentLike++;
                else if (row.Status == AttendanceStatus.Absent)
                    acc.absent++;

                weeklyMap[ws] = acc;
            }

            var weeklyTrend = new List<AttendanceWeeklyTrendItemDto>(weekCount);
            var currentWeekStart = StartOfWeek(today);

            for (var i = weekCount - 1; i >= 0; i--)
            {
                var ws = currentWeekStart.AddDays(-7 * i);
                weeklyMap.TryGetValue(ws, out var acc);

                var d = acc.presentLike + acc.absent;
                var pct = d > 0 ? (int)Math.Round((acc.presentLike * 100.0) / d) : 0;

                weeklyTrend.Add(new AttendanceWeeklyTrendItemDto
                {
                    WeekStart = ws,
                    Label = ws.ToDateTime(TimeOnly.MinValue).ToString("dd MMM", new CultureInfo("tr-TR")),
                    AttendancePercent = pct
                });
            }

            return new StudentDashboardDto
            {
                UpcomingSessions7DaysCount = upcomingSessionsCount,
                UpcomingSessions = upcomingSessions,

                MissingDocumentsCount = missingDocumentsCount,
                MissingDocuments = missingDocuments,

                OverdueFeesCount = overdueFeesCount,
                OverdueFeesAmount = overdueFeesAmount,
                OverdueFees = overdueFeesTop,

                DueSoonFeesCount = dueSoonFeesCount,
                DueSoonFeesAmount = dueSoonFeesAmount,
                DueSoonFees = dueSoonFeesTop,

                RecentAnnouncements = recentAnnouncements,

                AttendanceOverall = new AttendanceOverallDto
                {
                    PresentLikeCount = presentLike,
                    AbsentCount = overallAbsent,
                    ExcusedCount = overallExcused,
                    UnknownCount = overallUnknown,
                    AttendancePercent = overallPercent
                },
                AttendanceWeeklyTrend = weeklyTrend
            };
        }

        private static DateOnly StartOfWeek(DateOnly d)
        {
            var dow = d.DayOfWeek;
            var offset = dow == DayOfWeek.Sunday ? 6 : ((int)dow - 1);
            return d.AddDays(-offset);
        }
    }
}
