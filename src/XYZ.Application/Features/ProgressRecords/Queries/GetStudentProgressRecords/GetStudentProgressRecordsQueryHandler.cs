using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords
{
    public class GetStudentProgressRecordsQueryHandler
        : IRequestHandler<GetStudentProgressRecordsQuery, IList<ProgressRecordListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetStudentProgressRecordsQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<IList<ProgressRecordListItemDto>> Handle(
            GetStudentProgressRecordsQuery request,
            CancellationToken ct)
        {
            await _dataScope.EnsureStudentAccessAsync(request.StudentId, ct);

            var query = _dataScope.ProgressRecords()
                .Where(p => p.StudentId == request.StudentId);

            if (request.From.HasValue)
            {
                query = query.Where(p => p.RecordDate >= request.From.Value);
            }

            if (request.To.HasValue)
            {
                query = query.Where(p => p.RecordDate <= request.To.Value);
            }

            var list = await query
                .OrderByDescending(p => p.RecordDate)
                .ThenByDescending(p => p.Id)
                .Select(p => new ProgressRecordListItemDto
                {
                    Id = p.Id,
                    RecordDate = p.RecordDate,
                    Height = p.Height,
                    Weight = p.Weight,
                    BodyFatPercentage = p.BodyFatPercentage,
                    TechnicalScore = p.TechnicalScore,
                    TacticalScore = p.TacticalScore,
                    PhysicalScore = p.PhysicalScore,
                    MentalScore = p.MentalScore,
                    CoachNotes = p.CoachNotes
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return list;
        }
    }
}
