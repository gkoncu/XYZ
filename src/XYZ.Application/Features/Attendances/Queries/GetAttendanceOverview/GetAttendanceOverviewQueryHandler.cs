using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Attendances.Queries.GetAttendanceOverview
{
    public class GetAttendanceOverviewQueryHandler
        : IRequestHandler<GetAttendanceOverviewQuery, AttendanceOverviewDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetAttendanceOverviewQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<AttendanceOverviewDto> Handle(
            GetAttendanceOverviewQuery request,
            CancellationToken ct)
        {
            var from = request.From;
            var to = request.To;

            var clsInfo = await _dataScope.Classes()
                .Where(c => c.Id == request.ClassId)
                .Select(c => new { c.Id, c.Name })
                .SingleOrDefaultAsync(ct);

            if (clsInfo is null)
            {
                throw new NotFoundException("Class", request.ClassId);
            }

            var query =
                from a in _context.Attendances
                join cs in _context.ClassSessions on a.ClassSessionId equals cs.Id
                where a.ClassId == request.ClassId
                      && a.IsActive
                      && cs.IsActive
                      && cs.Date >= @from
                      && cs.Date <= to
                select new { a, cs };

            var stats = await query
                .GroupBy(x => 1)
                .Select(g => new
                {
                    TotalSessions = g.Select(x => x.cs.Id).Distinct().Count(),
                    TotalAttendanceRecords = g.Count(),

                    PresentCount = g.Count(x => x.a.Status == AttendanceStatus.Present),
                    AbsentCount = g.Count(x => x.a.Status == AttendanceStatus.Absent),
                    ExcusedCount = g.Count(x => x.a.Status == AttendanceStatus.Excused),
                    LateCount = g.Count(x => x.a.Status == AttendanceStatus.Late),
                    UnknownCount = g.Count(x => x.a.Status == AttendanceStatus.Unknown)
                })
                .SingleOrDefaultAsync(ct);

            var overview = new AttendanceOverviewDto
            {
                ClassId = clsInfo.Id,
                ClassName = clsInfo.Name,
                From = from,
                To = to
            };

            if (stats is not null)
            {
                overview.TotalSessions = stats.TotalSessions;
                overview.TotalAttendanceRecords = stats.TotalAttendanceRecords;
                overview.PresentCount = stats.PresentCount;
                overview.AbsentCount = stats.AbsentCount;
                overview.ExcusedCount = stats.ExcusedCount;
                overview.LateCount = stats.LateCount;
                overview.UnknownCount = stats.UnknownCount;

                if (stats.TotalAttendanceRecords > 0)
                {
                    overview.AttendanceRate =
                        (double)stats.PresentCount * 100.0 / stats.TotalAttendanceRecords;
                }
            }

            return overview;
        }
    }
}
