using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory
{
    public class GetStudentAttendanceHistoryQueryHandler
        : IRequestHandler<GetStudentAttendanceHistoryQuery, IList<StudentAttendanceHistoryItemDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetStudentAttendanceHistoryQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<IList<StudentAttendanceHistoryItemDto>> Handle(
            GetStudentAttendanceHistoryQuery request,
            CancellationToken ct)
        {
            var classesQuery = _dataScope.Classes();

            var query =
                from a in _context.Attendances
                join cs in _context.ClassSessions on a.ClassSessionId equals cs.Id
                join cls in classesQuery on a.ClassId equals cls.Id
                where a.StudentId == request.StudentId
                      && a.IsActive
                      && cs.IsActive
                select new { a, cs, cls };

            if (request.From.HasValue)
            {
                var from = request.From.Value;
                query = query.Where(x => x.cs.Date >= from);
            }

            if (request.To.HasValue)
            {
                var to = request.To.Value;
                query = query.Where(x => x.cs.Date <= to);
            }

            var list = await query
                .OrderByDescending(x => x.cs.Date)
                .ThenByDescending(x => x.cs.StartTime)
                .Select(x => new StudentAttendanceHistoryItemDto
                {
                    SessionId = x.cs.Id,
                    ClassId = x.cls.Id,
                    ClassName = x.cls.Name,
                    Date = x.cs.Date,
                    StartTime = x.cs.StartTime,
                    EndTime = x.cs.EndTime,
                    Status = x.a.Status,
                    Score = x.a.Score,
                    Note = x.a.Note,
                    CoachComment = x.a.CoachComment
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return list;
        }
    }
}
