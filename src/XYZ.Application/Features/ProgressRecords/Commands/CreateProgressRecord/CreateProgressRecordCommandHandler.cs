using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord
{
    public class CreateProgressRecordCommandHandler
        : IRequestHandler<CreateProgressRecordCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public CreateProgressRecordCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
        }

        public async Task<int> Handle(CreateProgressRecordCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Gelişim kaydı oluşturma yetkiniz yok.");

            // Öğrenci, erişebildiğin scope içinde mi?
            var student = await _dataScope.Students()
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student is null)
                throw new NotFoundException("Student", request.StudentId);

            var now = DateTime.UtcNow;
            var recordDate = request.RecordDate == default ? now : request.RecordDate;

            var entity = new ProgressRecord
            {
                StudentId = student.Id,
                RecordDate = recordDate,

                Height = request.Height,
                Weight = request.Weight,
                BodyFatPercentage = request.BodyFatPercentage,
                VerticalJump = request.VerticalJump,
                SprintTime = request.SprintTime,
                Endurance = request.Endurance,
                Flexibility = request.Flexibility,

                TechnicalScore = request.TechnicalScore,
                TacticalScore = request.TacticalScore,
                PhysicalScore = request.PhysicalScore,
                MentalScore = request.MentalScore,

                CoachNotes = request.CoachNotes,
                Goals = request.Goals,

                IsActive = true,
                CreatedAt = now
            };

            await _context.ProgressRecords.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return entity.Id;
        }
    }
}
