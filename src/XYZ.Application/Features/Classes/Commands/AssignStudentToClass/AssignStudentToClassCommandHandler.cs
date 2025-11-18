using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Classes.Commands.AssignStudentToClass
{
    public class AssignStudentToClassCommandHandler : IRequestHandler<AssignStudentToClassCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public AssignStudentToClassCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _dataScope = dataScope;
            _context = context;
            _current = currentUser;
        }

        public async Task<int> Handle(AssignStudentToClassCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Sınıfa öğrenci atama yetkiniz yok.");

            var student = await _dataScope.Students()
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student is null)
                throw new NotFoundException("Student", request.StudentId);

            var cls = await _dataScope.Classes()
                .FirstOrDefaultAsync(c => c.Id == request.ClassId, ct);

            if (cls is null)
                throw new NotFoundException("Class", request.ClassId);

            if (cls.TenantId != student.TenantId)
                throw new UnauthorizedAccessException("Öğrenci ve sınıf farklı tenant'a ait.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var existingEnrollment = await _context.ClassEnrollments
                .FirstOrDefaultAsync(e =>
                        e.StudentId == student.Id &&
                        e.ClassId == cls.Id &&
                        e.EndDate == null,
                    ct);

            if (existingEnrollment is null)
            {
                var enrollment = new ClassEnrollment
                {
                    StudentId = student.Id,
                    ClassId = cls.Id,
                    StartDate = today,
                    EndDate = null
                };

                await _context.ClassEnrollments.AddAsync(enrollment, ct);
            }
            else
            {
                if (existingEnrollment.StartDate > today)
                {
                    existingEnrollment.StartDate = today;
                }
            }

            if (student.ClassId != request.ClassId)
            {
                student.ClassId = request.ClassId;
                student.UpdatedAt = DateTime.UtcNow;
            }

            var futureSessions = await _context.ClassSessions
                .Where(cs =>
                    cs.ClassId == cls.Id &&
                    cs.Date >= today &&
                    cs.Status != SessionStatus.Cancelled &&
                    cs.IsActive)
                .ToListAsync(ct);

            if (futureSessions.Count > 0)
            {
                var futureSessionIds = futureSessions.Select(cs => cs.Id).ToList();

                var existingAttendanceSessionIds = await _dataScope.Attendances()
                    .Where(a =>
                        a.StudentId == student.Id &&
                        a.ClassId == cls.Id &&
                        futureSessionIds.Contains(a.ClassSessionId))
                    .Select(a => a.ClassSessionId)
                    .ToListAsync(ct);

                foreach (var session in futureSessions)
                {
                    if (existingAttendanceSessionIds.Contains(session.Id))
                        continue;

                    var attendance = new Attendance
                    {
                        ClassSessionId = session.Id,
                        ClassId = cls.Id,
                        StudentId = student.Id,
                        Status = AttendanceStatus.Unknown
                    };

                    await _context.Attendances.AddAsync(attendance, ct);
                }
            }

            await _context.SaveChangesAsync(ct);
            return student.Id;
        }
    }

}
