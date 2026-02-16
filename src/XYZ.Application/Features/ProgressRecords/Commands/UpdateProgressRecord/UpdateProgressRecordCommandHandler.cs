using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord
{
    public class UpdateProgressRecordCommandHandler : IRequestHandler<UpdateProgressRecordCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public UpdateProgressRecordCommandHandler(IApplicationDbContext context, IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(UpdateProgressRecordCommand request, CancellationToken ct)
        {
            var record = await _dataScope.ProgressRecords()
                .Include(r => r.Values)
                .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

            if (record is null)
                throw new KeyNotFoundException("Gelişim kaydı bulunamadı.");

            await _dataScope.EnsureBranchAccessAsync(record.BranchId, ct);

            var defs = await _context.ProgressMetricDefinitions
                .Where(d => d.BranchId == record.BranchId && d.IsActive)
                .ToListAsync(ct);

            if (defs.Count == 0)
                throw new InvalidOperationException("Bu branş için gelişim metrikleri tanımlanmamış.");

            var defsById = defs.ToDictionary(d => d.Id);

            record.CoachNotes = request.CoachNotes;
            record.Goals = request.Goals;
            record.UpdatedAt = DateTime.UtcNow;

            _context.ProgressRecordValues.RemoveRange(record.Values);
            record.Values.Clear();

            var now = DateTime.UtcNow;

            foreach (var input in request.Values)
            {
                if (!defsById.TryGetValue(input.ProgressMetricDefinitionId, out var def))
                    throw new InvalidOperationException("Seçilen metrik bu branşa ait değil veya pasif.");

                if (!IsValueCompatible(def.DataType, input))
                    throw new InvalidOperationException($"'{def.Name}' metriği için girilen değer tipi hatalı.");

                if (def.DataType is ProgressMetricDataType.Decimal or ProgressMetricDataType.Int)
                {
                    var numeric = def.DataType == ProgressMetricDataType.Decimal
                        ? input.DecimalValue
                        : (input.IntValue.HasValue ? (decimal?)input.IntValue.Value : null);

                    if (numeric.HasValue)
                    {
                        if (def.MinValue.HasValue && numeric.Value < def.MinValue.Value)
                            throw new InvalidOperationException($"'{def.Name}' için değer en az {def.MinValue} olmalıdır.");

                        if (def.MaxValue.HasValue && numeric.Value > def.MaxValue.Value)
                            throw new InvalidOperationException($"'{def.Name}' için değer en fazla {def.MaxValue} olmalıdır.");
                    }
                }

                if (input.DecimalValue is null && input.IntValue is null && string.IsNullOrWhiteSpace(input.TextValue))
                    continue;

                record.Values.Add(new ProgressRecordValue
                {
                    ProgressMetricDefinitionId = def.Id,
                    DecimalValue = input.DecimalValue,
                    IntValue = input.IntValue,
                    TextValue = string.IsNullOrWhiteSpace(input.TextValue) ? null : input.TextValue.Trim(),
                    CreatedAt = now,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync(ct);
            return record.Id;
        }

        private bool IsValueCompatible(ProgressMetricDataType type, MetricValueInput input)
        {
            return type switch
            {
                ProgressMetricDataType.Decimal => input.IntValue is null && string.IsNullOrWhiteSpace(input.TextValue),
                ProgressMetricDataType.Int => input.DecimalValue is null && string.IsNullOrWhiteSpace(input.TextValue),
                ProgressMetricDataType.Text => input.DecimalValue is null && input.IntValue is null,
                _ => false
            };
        }
    }
}
