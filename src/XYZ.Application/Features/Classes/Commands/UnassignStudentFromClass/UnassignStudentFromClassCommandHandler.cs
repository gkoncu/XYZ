using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Classes.Commands.UnassignStudentFromClass
{
    public class UnassignStudentFromClassCommandHandler
        : IRequestHandler<UnassignStudentFromClassCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly IPermissionService _permissions;

        public UnassignStudentFromClassCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            IPermissionService permissions)
        {
            _dataScope = dataScope;
            _context = context;
            _permissions = permissions;
        }

        public async Task<int> Handle(UnassignStudentFromClassCommand request, CancellationToken ct)
        {
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

            var canUpdateFutureAttendanceList =
                await _permissions.HasPermissionAsync(PermissionNames.Classes.EnrollStudents, PermissionScope.OwnClasses, ct)
                || await _permissions.HasPermissionAsync(PermissionNames.Classes.UnenrollStudents, PermissionScope.OwnClasses, ct);

            if (canUpdateFutureAttendanceList)
            {
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
            }

            await _context.SaveChangesAsync(ct);

            return student.Id;
        }
    }
}
