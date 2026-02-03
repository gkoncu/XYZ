using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetProgressRecordById
{
    public class GetProgressRecordByIdQueryHandler : IRequestHandler<GetProgressRecordByIdQuery, ProgressRecordDetailDto>
    {
        private readonly IDataScopeService _dataScope;

        public GetProgressRecordByIdQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<ProgressRecordDetailDto> Handle(GetProgressRecordByIdQuery request, CancellationToken ct)
        {
            var record = await _dataScope.ProgressRecords()
                .Include(r => r.Branch)
                .Include(r => r.Values)
                    .ThenInclude(v => v.ProgressMetricDefinition)
                .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

            if (record is null)
                throw new NotFoundException("ProgressRecord", request.Id);

            return new ProgressRecordDetailDto
            {
                Id = record.Id,
                StudentId = record.StudentId,
                BranchId = record.BranchId,
                BranchName = record.Branch?.Name,
                RecordDate = record.RecordDate,
                Sequence = record.Sequence,
                CreatedByDisplayName = record.CreatedByDisplayName,
                CoachNotes = record.CoachNotes,
                Goals = record.Goals,
                Values = record.Values
                    .OrderBy(v => v.ProgressMetricDefinition.SortOrder)
                    .Select(v => new ProgressRecordMetricValueDto
                    {
                        ProgressMetricDefinitionId = v.ProgressMetricDefinitionId,
                        MetricName = v.ProgressMetricDefinition.Name,
                        DataType = v.ProgressMetricDefinition.DataType,
                        Unit = v.ProgressMetricDefinition.Unit,
                        DecimalValue = v.DecimalValue,
                        IntValue = v.IntValue,
                        TextValue = v.TextValue
                    })
                    .ToList()
            };
        }
    }
}
