using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;

public sealed class GetAdminCoachDashboardQueryHandler
    : IRequestHandler<GetAdminCoachDashboardQuery, AdminCoachDashboardDto>
{
    private readonly IDataScopeService _dataScope;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _current;
    private readonly IPermissionService _permissions;

    public GetAdminCoachDashboardQueryHandler(
        IDataScopeService dataScope,
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IPermissionService permissions)
    {
        _dataScope = dataScope;
        _context = context;
        _current = currentUser;
        _permissions = permissions;
    }

    public async Task<AdminCoachDashboardDto> Handle(
        GetAdminCoachDashboardQuery request,
        CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var nowUtc = DateTime.UtcNow;

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

        var todaySessions = await _context.ClassSessions
            .AsNoTracking()
            .Where(cs => cs.IsActive && cs.Date == today && cs.Status == SessionStatus.Scheduled)
            .Join(scopedClasses,
                cs => cs.ClassId,
                c => c.Id,
                (cs, c) => new TodaySessionListItemDto
                {
                    SessionId = cs.Id,
                    ClassId = cs.ClassId,
                    ClassName = c.Name,
                    Date = cs.Date,
                    StartTime = cs.StartTime,
                    EndTime = cs.EndTime,
                    Title = cs.Title,
                    Location = cs.Location
                })
            .OrderBy(x => x.StartTime)
            .ThenBy(x => x.ClassName)
            .Take(8)
            .ToListAsync(ct);

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

        var overduePaymentsCount = 0;
        var overduePaymentsAmount = 0m;
        var overdueStudents = new List<OverdueStudentListItemDto>();

        var canReadPayments = await _permissions.HasPermissionAsync(
            PermissionNames.Payments.Read,
            PermissionScope.OwnClasses,
            ct);

        if (canReadPayments)
        {
            var overduePaymentsQuery = _dataScope.Payments()
                .Where(p => p.Status == PaymentStatus.Overdue);

            overduePaymentsCount = await overduePaymentsQuery.CountAsync(ct);

            overduePaymentsAmount = overduePaymentsCount == 0
                ? 0m
                : await overduePaymentsQuery
                    .Select(p => (decimal?)(p.Amount - (p.DiscountAmount ?? 0m)))
                    .SumAsync(ct) ?? 0m;

            var overdueGrouped = await overduePaymentsQuery
                .AsNoTracking()
                .GroupBy(p => p.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    OverdueCount = g.Count(),
                    OverdueAmount = g.Sum(x => x.Amount - (x.DiscountAmount ?? 0m)),
                    OldestDueDate = g.Min(x => (DateTime?)x.DueDate)
                })
                .OrderByDescending(x => x.OverdueAmount)
                .ThenByDescending(x => x.OverdueCount)
                .Take(5)
                .ToListAsync(ct);

            var overdueStudentIds = overdueGrouped.Select(x => x.StudentId).ToList();

            var overdueNames = await _dataScope.Students()
                .AsNoTracking()
                .Where(s => overdueStudentIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    FullName = (s.User.FirstName + " " + s.User.LastName).Trim()
                })
                .ToListAsync(ct);

            var overdueNameMap = overdueNames.ToDictionary(x => x.Id, x => x.FullName);

            overdueStudents = overdueGrouped
                .Select(x => new OverdueStudentListItemDto
                {
                    StudentId = x.StudentId,
                    FullName = overdueNameMap.TryGetValue(x.StudentId, out var n) ? n : $"#{x.StudentId}",
                    OverdueCount = x.OverdueCount,
                    OverdueAmount = x.OverdueAmount,
                    OldestDueDate = x.OldestDueDate
                })
                .ToList();
        }

        var tenantId = _current.TenantId;
        var incompleteStudentDocumentsCount = 0;
        var incompleteStudents = new List<IncompleteStudentListItemDto>();

        if (tenantId.HasValue)
        {
            (incompleteStudentDocumentsCount, incompleteStudents) =
                await GetIncompleteStudentsTopList(tenantId.Value, ct);
        }

        DateTime? subscriptionEnd = null;
        int? subscriptionDaysRemaining = null;

        var canManageTenantSettings = await _permissions.HasPermissionAsync(
            PermissionNames.Settings.TenantSettingsManage,
            PermissionScope.Tenant,
            ct);

        if (canManageTenantSettings && tenantId.HasValue)
        {
            var t = await _context.Tenants
                .AsNoTracking()
                .Where(x => x.Id == tenantId.Value)
                .Select(x => new { x.SubscriptionEndDate })
                .FirstOrDefaultAsync(ct);

            if (t is not null)
            {
                subscriptionEnd = t.SubscriptionEndDate;
                subscriptionDaysRemaining =
                    (int)Math.Floor((t.SubscriptionEndDate.Date - DateTime.Today).TotalDays);
            }
        }

        return new AdminCoachDashboardDto
        {
            StudentCount = studentCount,
            ClassCount = classCount,
            TodaySessionsCount = todaySessionsCount,
            UpcomingSessionsCount = upcomingSessionsCount,

            TodaySessions = todaySessions,
            RecentAnnouncements = recentAnnouncements,

            OverduePaymentsCount = overduePaymentsCount,
            OverduePaymentsAmount = overduePaymentsAmount,
            OverdueStudents = overdueStudents,

            IncompleteStudentDocumentsCount = incompleteStudentDocumentsCount,
            IncompleteStudents = incompleteStudents,

            SubscriptionEndDate = subscriptionEnd,
            SubscriptionDaysRemaining = subscriptionDaysRemaining
        };
    }

    private async Task<(int Count, List<IncompleteStudentListItemDto> TopList)> GetIncompleteStudentsTopList(
            int tenantId,
            CancellationToken ct)
    {
        var requiredDefinitionIds = await _context.DocumentDefinitions
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId
                        && d.Target == DocumentTarget.Student
                        && d.IsRequired)
            .Select(d => d.Id)
            .ToListAsync(ct);

        if (requiredDefinitionIds.Count == 0)
            return (0, new List<IncompleteStudentListItemDto>());

        var studentIds = await _dataScope.Students()
            .AsNoTracking()
            .Select(s => s.Id)
            .ToListAsync(ct);

        if (studentIds.Count == 0)
            return (0, new List<IncompleteStudentListItemDto>());

        var requiredCount = requiredDefinitionIds.Count;

        var uploaded = await _dataScope.Documents()
            .AsNoTracking()
            .Where(d => d.StudentId != null
                        && studentIds.Contains(d.StudentId.Value)
                        && requiredDefinitionIds.Contains(d.DocumentDefinitionId))
            .Select(d => new { OwnerId = d.StudentId!.Value, d.DocumentDefinitionId })
            .Distinct()
            .GroupBy(x => x.OwnerId)
            .Select(g => new { OwnerId = g.Key, UploadedCount = g.Count() })
            .ToListAsync(ct);

        var uploadedMap = uploaded.ToDictionary(x => x.OwnerId, x => x.UploadedCount);

        var incomplete = studentIds
            .Select(id =>
            {
                var up = uploadedMap.TryGetValue(id, out var c) ? c : 0;
                var missing = Math.Max(0, requiredCount - up);
                return new { id, missing };
            })
            .Where(x => x.missing > 0)
            .ToList();

        var count = incomplete.Count;
        if (count == 0)
            return (0, new List<IncompleteStudentListItemDto>());

        var top = incomplete
            .OrderByDescending(x => x.missing)
            .ThenBy(x => x.id)
            .Take(5)
            .ToList();

        var topIds = top.Select(x => x.id).ToList();

        var names = await _dataScope.Students()
            .AsNoTracking()
            .Where(s => topIds.Contains(s.Id))
            .Select(s => new
            {
                s.Id,
                FullName = (s.User.FirstName + " " + s.User.LastName).Trim()
            })
            .ToListAsync(ct);

        var nameMap = names.ToDictionary(x => x.Id, x => x.FullName);

        var topList = top.Select(x => new IncompleteStudentListItemDto
        {
            StudentId = x.id,
            FullName = nameMap.TryGetValue(x.id, out var n) ? n : $"#{x.id}",
            MissingCount = x.missing
        }).ToList();

        return (count, topList);
    }
}

