using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Students.Commands.ActivateStudent
{
    public class ActivateStudentCommandHandler : IRequestHandler<ActivateStudentCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;

        public ActivateStudentCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context)
        {
            _dataScope = dataScope;
            _context = context;
        }

        public async Task<int> Handle(ActivateStudentCommand request, CancellationToken ct)
        {
            var student = await _dataScope.Students()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student is null)
                throw new NotFoundException("Student", request.StudentId);

            if (student.IsActive && student.User.IsActive)
                return student.Id;

            student.IsActive = true;
            student.UpdatedAt = DateTime.UtcNow;

            student.User.IsActive = true;
            student.User.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return student.Id;
        }
    }
}
