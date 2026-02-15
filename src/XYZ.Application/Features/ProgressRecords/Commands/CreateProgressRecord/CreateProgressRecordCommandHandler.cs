using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord
{
    public class CreateProgressRecordCommandHandler : IRequestHandler<CreateProgressRecordCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public CreateProgressRecordCommandHandler(IApplicationDbContext context, IDataScopeService dataScope, ICurrentUserService current)
        {
            _context = context;
            _dataScope = dataScope;
            _current = current;
        }

        public async Task<int> Handle(CreateProgressRecordCommand request, CancellationToken ct)
        {
            var student = await _dataScope.Students()
                .Include(s => s.Tenant)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student is null)
                throw new KeyNotFoundException("Öğrenci bulunamadı.");

            await _dataScope.EnsureBranchAccessAsync(request.BranchId, ct);

            var branch = await _dataScope.Branches()
                .FirstOrDefaultAsync(b => b.Id == request.BranchId, ct);

            if (branch is null)
                throw new KeyNotFoundException("Branş bulunamadı.");

            if (branch.TenantId != student.TenantId)
                throw new InvalidOperationException("Seçilen branş bu öğrencinin kulübüne ait değil.");

            var maxSeq = await _dataScope.ProgressRecords()
                .Where(r => r.StudentId == request.StudentId
                            && r.BranchId == request.BranchId
                            && r.RecordDate == request.RecordDate)
                .Select(r => (int?)r.Sequence)
                .MaxAsync(ct);

            var nextSeq = (maxSeq ?? 0) + 1;

            var defs = await _context.ProgressMetricDefinitions
                .Where(d => d.BranchId == request.BranchId && d.IsActive)
                .ToListAsync(ct);

            if (defs.Count == 0)
                throw new InvalidOperationException("Bu branş için gelişim metrikleri tanımlanmamış.");

            var defsById = defs.ToDictionary(d => d.Id);

            var now = DateTime.UtcNow;

            var entity = new ProgressRecord
            {
                TenantId = student.TenantId,
                StudentId = request.StudentId,
                BranchId = request.BranchId,
                RecordDate = request.RecordDate,
                Sequence = nextSeq,

                CreatedByUserId = _current.UserId,
                CreatedByDisplayName = await ResolveDisplayNameAsync(ct),

                CoachNotes = request.CoachNotes,
                Goals = request.Goals,

                CreatedAt = now,
                IsActive = true
            };

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

                entity.Values.Add(new ProgressRecordValue
                {
                    TenantId = student.TenantId,
                    ProgressMetricDefinitionId = def.Id,
                    DecimalValue = input.DecimalValue,
                    IntValue = input.IntValue,
                    TextValue = string.IsNullOrWhiteSpace(input.TextValue) ? null : input.TextValue.Trim(),
                    CreatedAt = now,
                    IsActive = true
                });
            }

            await _context.ProgressRecords.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return entity.Id;
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

        private async Task<string?> ResolveDisplayNameAsync(CancellationToken ct)
        {
            var userId = _current.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return null;

            var fullName = user.FullName?.Trim();
            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;

            var email = user.Email?.Trim();
            return string.IsNullOrWhiteSpace(email) ? null : email;
        }
    }
}
