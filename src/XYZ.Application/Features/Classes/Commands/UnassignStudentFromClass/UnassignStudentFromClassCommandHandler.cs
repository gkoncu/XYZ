using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Classes.Commands.UnassignStudentFromClass
{
    public class UnassignStudentFromClassCommandHandler
        : IRequestHandler<UnassignStudentFromClassCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public UnassignStudentFromClassCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(
            UnassignStudentFromClassCommand request,
            CancellationToken cancellationToken)
        {
            var student = await _dataScope.Students()
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);

            if (student is null)
                throw new NotFoundException("Student", request.StudentId);

            if (!student.ClassId.HasValue)
                throw new InvalidOperationException("Öğrencinin atanmış bir sınıfı yok.");

            if (request.ClassId.HasValue && student.ClassId != request.ClassId.Value)
                throw new InvalidOperationException("Öğrenci bu sınıfa kayıtlı değil.");

            student.ClassId = null;
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return student.Id;
        }
    }
}
