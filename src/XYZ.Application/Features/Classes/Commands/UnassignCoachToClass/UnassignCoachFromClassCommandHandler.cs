using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Classes.Commands.UnassignCoachToClass
{
    public class UnassignCoachFromClassCommandHandler
        : IRequestHandler<UnassignCoachFromClassCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public UnassignCoachFromClassCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(UnassignCoachFromClassCommand request, CancellationToken cancellationToken)
        {
            if (!request.ClassId.HasValue)
                throw new InvalidOperationException("ClassId gereklidir.");

            var @class = await _dataScope.Classes()
                .Include(c => c.Coaches)
                .FirstOrDefaultAsync(c => c.Id == request.ClassId.Value, cancellationToken);

            if (@class is null)
                throw new NotFoundException("Class", request.ClassId!.Value);

            var coach = @class.Coaches.FirstOrDefault(c => c.Id == request.CoachId);
            if (coach is null)
                throw new InvalidOperationException("Koç bu sınıfa atanmış değil.");

            @class.Coaches.Remove(coach);
            await _context.SaveChangesAsync(cancellationToken);

            return @class.Id;
        }
    }
}
