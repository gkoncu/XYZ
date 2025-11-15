using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Classes.Commands.DeleteClass
{
    public class DeleteClassCommandHandler : IRequestHandler<DeleteClassCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public DeleteClassCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
        {
            var @class = await _dataScope.Classes()
                .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);

            if (@class is null)
                throw new NotFoundException("Class", request.ClassId);

            var hasActiveStudents = await _dataScope.Students()
                .AnyAsync(s =>
                    s.ClassId == @class.Id &&
                    s.IsActive &&
                    s.User.IsActive,
                    cancellationToken);

            if (hasActiveStudents)
                throw new InvalidOperationException("Bu sınıfa atanmış aktif öğrenciler olduğu için silinemez. Önce öğrencilerin sınıf atamasını kaldırın.");

            @class.IsActive = false;
            @class.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return @class.Id;
        }
    }
}
