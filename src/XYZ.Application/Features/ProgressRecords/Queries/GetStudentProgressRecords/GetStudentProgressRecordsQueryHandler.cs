using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords
{
    public class GetStudentProgressRecordsQueryHandler : IRequestHandler<GetStudentProgressRecordsQuery, IList<ProgressRecordListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetStudentProgressRecordsQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<IList<ProgressRecordListItemDto>> Handle(GetStudentProgressRecordsQuery request, CancellationToken ct)
        {
            await _dataScope.EnsureStudentAccessAsync(request.StudentId, ct);

            var q = _dataScope.ProgressRecords()
                .Where(r => r.StudentId == request.StudentId)
                .Include(r => r.Branch)
                .Include(r => r.Values)
                .AsQueryable();

            if (request.BranchId.HasValue)
                q = q.Where(r => r.BranchId == request.BranchId.Value);

            if (request.From.HasValue)
                q = q.Where(r => r.RecordDate >= request.From.Value);

            if (request.To.HasValue)
                q = q.Where(r => r.RecordDate <= request.To.Value);

            return await q
                .OrderByDescending(r => r.RecordDate)
                .ThenByDescending(r => r.Sequence)
                .Select(r => new ProgressRecordListItemDto
                {
                    Id = r.Id,
                    BranchId = r.BranchId,
                    BranchName = r.Branch.Name,
                    RecordDate = r.RecordDate,
                    Sequence = r.Sequence,
                    CreatedByDisplayName = r.CreatedByDisplayName,
                    FilledMetricsCount = r.Values.Count(v =>
                        v.DecimalValue != null || v.IntValue != null || v.TextValue != null)
                })
                .ToListAsync(ct);
        }
    }
}
