using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord
{
    public class UpdateProgressRecordCommandHandler
        : IRequestHandler<UpdateProgressRecordCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public UpdateProgressRecordCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _dataScope = dataScope;
            _context = context;
            _current = currentUser;
        }

        public async Task<int> Handle(UpdateProgressRecordCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Gelişim kaydı güncelleme yetkiniz yok.");

            var entity = await _dataScope.ProgressRecords()
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (entity is null)
                throw new NotFoundException("ProgressRecord", request.Id);

            entity.RecordDate = request.RecordDate == default
                ? entity.RecordDate
                : request.RecordDate;

            entity.Height = request.Height;
            entity.Weight = request.Weight;
            entity.BodyFatPercentage = request.BodyFatPercentage;
            entity.VerticalJump = request.VerticalJump;
            entity.SprintTime = request.SprintTime;
            entity.Endurance = request.Endurance;
            entity.Flexibility = request.Flexibility;

            entity.TechnicalScore = request.TechnicalScore;
            entity.TacticalScore = request.TacticalScore;
            entity.PhysicalScore = request.PhysicalScore;
            entity.MentalScore = request.MentalScore;

            entity.CoachNotes = request.CoachNotes;
            entity.Goals = request.Goals;

            if (request.IsActive.HasValue)
                entity.IsActive = request.IsActive.Value;

            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }
    }
}
