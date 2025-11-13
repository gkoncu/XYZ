using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

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
                .Where(c => c.Id == request.ClassId)
                .Select(c => new { c.Id, c.TenantId })
                .FirstOrDefaultAsync(ct);

            if (cls is null)
                throw new NotFoundException("Class", request.ClassId);

            if (cls.TenantId != student.TenantId)
                throw new UnauthorizedAccessException("Öğrenci ve sınıf farklı tenant'a ait.");

            if (student.ClassId == request.ClassId)
                return student.Id;

            student.ClassId = request.ClassId;
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return student.Id;
        }
    }

}
