using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Data;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Commands.CreateClassSession
{
    public sealed record CreateClassSessionCommand(
        int ClassId,
        DateOnly Date,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Title,
        string? Description,
        string? Location,
        bool GenerateAttendance = true
    ) : IRequest<int>;

    public sealed class CreateClassSessionCommandHandler
        : IRequestHandler<CreateClassSessionCommand, int>
    {
        private readonly ApplicationDbContext _db;

        public CreateClassSessionCommandHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> Handle(CreateClassSessionCommand request, CancellationToken cancellationToken)
        {
            var @class = await _db.Classes
                .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);

            if (@class is null)
            {
                throw new NotFoundException($"Class not found. Id = {request.ClassId}");
            }

            var session = new ClassSession
            {
                ClassId = request.ClassId,
                Date = request.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Title = request.Title,
                Description = request.Description,
                Location = request.Location,
                Status = SessionStatus.Scheduled
            };

            if (request.GenerateAttendance)
            {
                var activeEnrollments = await _db.ClassEnrollments
                    .Where(e => e.ClassId == request.ClassId &&
                                e.StartDate <= request.Date &&
                               (e.EndDate == null || e.EndDate >= request.Date))
                    .ToListAsync(cancellationToken);

                foreach (var enrollment in activeEnrollments)
                {
                    var attendance = new Attendance
                    {
                        ClassSession = session,
                        ClassId = request.ClassId,
                        StudentId = enrollment.StudentId,
                        Status = AttendanceStatus.Unknown
                    };

                    session.Attendances.Add(attendance);
                }
            }

            _db.ClassSessions.Add(session);

            await _db.SaveChangesAsync(cancellationToken);

            return session.Id;
        }
    }
}
