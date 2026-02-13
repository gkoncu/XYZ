using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
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

    public sealed class CreateClassSessionCommandHandler(
        ApplicationDbContext db,
        IDataScopeService dataScope
    ) : IRequestHandler<CreateClassSessionCommand, int>
    {
        public async Task<int> Handle(CreateClassSessionCommand request, CancellationToken cancellationToken)
        {
            var @class = await dataScope.Classes()
                .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);

            if (@class is null)
            {
                throw new NotFoundException($"Class not found. Id = {request.ClassId}");
            }

            var session = new ClassSession
            {
                TenantId = @class.TenantId,
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
                var activeEnrollments = await db.ClassEnrollments
                    .Where(e => e.ClassId == request.ClassId &&
                                e.StartDate <= request.Date &&
                               (e.EndDate == null || e.EndDate >= request.Date))
                    .ToListAsync(cancellationToken);

                foreach (var enrollment in activeEnrollments)
                {
                    var attendance = new Attendance
                    {
                        TenantId = @class.TenantId,
                        ClassSession = session,
                        ClassId = request.ClassId,
                        StudentId = enrollment.StudentId,
                        Status = AttendanceStatus.Unknown
                    };

                    session.Attendances.Add(attendance);
                }
            }

            db.ClassSessions.Add(session);

            await db.SaveChangesAsync(cancellationToken);

            return session.Id;
        }
    }
}
