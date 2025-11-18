using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Attendances.Queries.GetSessionAttendance
{
    public class GetSessionAttendanceQueryHandler
        : IRequestHandler<GetSessionAttendanceQuery, SessionAttendanceDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetSessionAttendanceQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<SessionAttendanceDto> Handle(
            GetSessionAttendanceQuery request,
            CancellationToken ct)
        {
            var session = await _context.ClassSessions
                .Include(cs => cs.Class)
                .FirstOrDefaultAsync(cs => cs.Id == request.ClassSessionId, ct);

            if (session is null)
                throw new NotFoundException("ClassSession", request.ClassSessionId);

            await _dataScope.EnsureClassAccessAsync(session.ClassId, ct);

            var attendances = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Where(a =>
                    a.ClassSessionId == session.Id &&
                    a.IsActive)
                .OrderBy(a => a.Student.User.FullName)
                .ToListAsync(ct);

            var dto = new SessionAttendanceDto
            {
                SessionId = session.Id,
                ClassId = session.ClassId,
                ClassName = session.Class.Name,
                Date = session.Date,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Title = session.Title,
                Location = session.Location,
                Status = session.Status,
                Students = attendances.Select(a => new SessionAttendanceStudentDto
                {
                    AttendanceId = a.Id,
                    StudentId = a.StudentId,
                    FullName = a.Student.User.FullName,
                    Status = a.Status,
                    Note = a.Note,
                    Score = a.Score,
                    CoachComment = a.CoachComment
                }).ToList()
            };

            return dto;
        }
    }
}
