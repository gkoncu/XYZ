using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Coaches.Commands.DeleteCoach
{
    public class DeleteCoachCommandHandler : IRequestHandler<DeleteCoachCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public DeleteCoachCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(DeleteCoachCommand request, CancellationToken cancellationToken)
        {
            var coach = await _dataScope.Coaches()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == request.CoachId, cancellationToken);

            if (coach is null)
                throw new NotFoundException("Coach", request.CoachId);

            var hasActiveClasses = await _dataScope.Classes()
                .AnyAsync(c =>
                    c.IsActive &&
                    c.Coaches.Any(co => co.Id == coach.Id),
                    cancellationToken);

            if (hasActiveClasses)
                throw new InvalidOperationException("Bu koça atanmış aktif sınıflar olduğu için silinemez. Önce sınıf-koç ilişkilerini kaldırın.");

            coach.IsActive = false;
            coach.UpdatedAt = DateTime.UtcNow;
            coach.User.IsActive = false;
            coach.User.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return coach.Id;
        }
    }
}
