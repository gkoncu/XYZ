using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Classes.Commands.AssignCoachToClass
{
    public class AssignCoachToClassCommandHandler
        : IRequestHandler<AssignCoachToClassCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public AssignCoachToClassCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(AssignCoachToClassCommand request, CancellationToken cancellationToken)
        {
            var @class = await _dataScope.Classes()
                .Include(c => c.Coaches)
                .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);

            if (@class is null)
                throw new NotFoundException("Class", request.ClassId);

            var coach = await _dataScope.Coaches()
                .FirstOrDefaultAsync(c => c.Id == request.CoachId, cancellationToken);

            if (coach is null)
                throw new NotFoundException("Coach", request.CoachId);

            if (@class.TenantId != coach.TenantId)
                throw new UnauthorizedAccessException("Koç ve sınıf aynı tenant'a ait olmalıdır.");

            if (coach.BranchId != @class.BranchId)
                throw new InvalidOperationException("Koç sadece kendi branşına ait sınıflara atanabilir.");

            if (@class.Coaches.Any(c => c.Id == coach.Id))
                return @class.Id;

            @class.Coaches.Add(coach);
            await _context.SaveChangesAsync(cancellationToken);

            return @class.Id;
        }
    }
}
