using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Queries.GetMyTodaySessions
{
    public class GetMyTodaySessionsQueryHandler
        : IRequestHandler<GetMyTodaySessionsQuery, IList<MyTodaySessionListItemDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetMyTodaySessionsQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<IList<MyTodaySessionListItemDto>> Handle(
            GetMyTodaySessionsQuery request,
            CancellationToken ct)
        {
            var targetDate = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var classIds = await _dataScope.Classes()
                .Select(c => c.Id)
                .ToListAsync(ct);

            if (classIds.Count == 0)
                return new List<MyTodaySessionListItemDto>();

            var query = _context.ClassSessions
                .AsNoTracking()
                .Include(cs => cs.Class)
                    .ThenInclude(c => c.Branch)
                .Include(cs => cs.Attendances)
                .Where(cs =>
                    cs.IsActive &&
                    cs.Status != SessionStatus.Cancelled &&
                    cs.Date == targetDate &&
                    classIds.Contains(cs.ClassId));

            var list = await query
                .Select(cs => new MyTodaySessionListItemDto
                {
                    SessionId = cs.Id,
                    ClassId = cs.ClassId,
                    ClassName = cs.Class.Name,
                    BranchName = cs.Class.Branch != null
                        ? cs.Class.Branch.Name
                        : null,
                    Date = cs.Date,
                    StartTime = cs.StartTime,
                    EndTime = cs.EndTime,
                    Title = cs.Title,
                    Status = cs.Status,
                    HasAttendance = cs.Attendances.Any(),
                    TotalStudents = cs.Attendances.Count,
                    PresentCount = cs.Attendances.Count(a => a.Status == AttendanceStatus.Present),
                    AbsentCount = cs.Attendances.Count(a =>
                        a.Status == AttendanceStatus.Absent ||
                        a.Status == AttendanceStatus.Excused)
                })
                .OrderBy(x => x.StartTime)
                .ToListAsync(ct);

            return list;
        }
    }
}
