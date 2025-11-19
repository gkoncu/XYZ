using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetProgressRecordById
{
    public class GetProgressRecordByIdQueryHandler
        : IRequestHandler<GetProgressRecordByIdQuery, ProgressRecordDetailDto>
    {
        private readonly IDataScopeService _dataScope;

        public GetProgressRecordByIdQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<ProgressRecordDetailDto> Handle(
            GetProgressRecordByIdQuery request,
            CancellationToken ct)
        {
            var entity = await _dataScope.ProgressRecords()
                .Include(p => p.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (entity is null)
                throw new NotFoundException("ProgressRecord", request.Id);

            return new ProgressRecordDetailDto
            {
                Id = entity.Id,
                StudentId = entity.StudentId,
                StudentFullName = entity.Student.User.FullName,
                RecordDate = entity.RecordDate,
                Height = entity.Height,
                Weight = entity.Weight,
                BodyFatPercentage = entity.BodyFatPercentage,
                VerticalJump = entity.VerticalJump,
                SprintTime = entity.SprintTime,
                Endurance = entity.Endurance,
                Flexibility = entity.Flexibility,
                TechnicalScore = entity.TechnicalScore,
                TacticalScore = entity.TacticalScore,
                PhysicalScore = entity.PhysicalScore,
                MentalScore = entity.MentalScore,
                CoachNotes = entity.CoachNotes,
                Goals = entity.Goals
            };
        }
    }
}
