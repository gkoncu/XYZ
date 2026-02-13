using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;

namespace XYZ.Application.Features.Classes.Commands.UnassignStudentFromClass
{
    public class UnassignStudentFromClassCommandHandler
        : IRequestHandler<UnassignStudentFromClassCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public UnassignStudentFromClassCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _dataScope = dataScope;
            _context = context;
            _current = currentUser;
        }

        public async Task<int> Handle(UnassignStudentFromClassCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || role is not (RoleNames.Admin or RoleNames.Coach or RoleNames.SuperAdmin))
                throw new UnauthorizedAccessException("Sınıftan öğrenci çıkarma yetkiniz yok.");

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
            var endDate = today;

            var enrollment = await _context.ClassEnrollments
                .FirstOrDefaultAsync(e =>
                        e.StudentId == student.Id &&
                        e.ClassId == cls.Id &&
                        e.EndDate == null,
                    ct);

            if (enrollment != null)
            {
                if (endDate < enrollment.StartDate)
                {
                    endDate = enrollment.StartDate;
                }

                enrollment.EndDate = endDate;
            }

            if (student.ClassId == cls.Id)
            {
                student.ClassId = null;
                student.UpdatedAt = DateTime.UtcNow;
            }

            var futureAttendancesQuery = _dataScope.Attendances()
                .Include(a => a.ClassSession)
                .Where(a =>
                    a.StudentId == student.Id &&
                    a.ClassId == cls.Id);

            if (enrollment != null)
            {
                futureAttendancesQuery = futureAttendancesQuery
                    .Where(a => a.ClassSession.Date > endDate);
            }
            else
            {
                futureAttendancesQuery = futureAttendancesQuery
                    .Where(a => a.ClassSession.Date > today);
            }

            var futureAttendances = await futureAttendancesQuery.ToListAsync(ct);

            foreach (var attendance in futureAttendances)
            {
                attendance.IsActive = false;
            }

            await _context.SaveChangesAsync(ct);

            return student.Id;
        }
    }
}
