using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetCoachesDocumentStatus
{
    public class GetCoachesDocumentStatusQueryHandler
        : IRequestHandler<GetCoachesDocumentStatusQuery, IList<CoachDocumentStatusListItemDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public GetCoachesDocumentStatusQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService current)
        {
            _context = context;
            _dataScope = dataScope;
            _current = current;
        }

        public async Task<IList<CoachDocumentStatusListItemDto>> Handle(GetCoachesDocumentStatusQuery request, CancellationToken ct)
        {
            var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var requiredIds = await _context.DocumentDefinitions
                .Where(d => d.TenantId == tenantId
                            && d.Target == DocumentTarget.Coach
                            && d.IsActive
                            && d.IsRequired)
                .Select(d => d.Id)
                .ToListAsync(ct);

            IQueryable<Coach> coachesQ = _dataScope.Coaches().Include(c => c.User);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var st = request.SearchTerm.Trim();
                coachesQ = coachesQ.Where(c => c.User.FullName.Contains(st));
            }


            var coaches = await coachesQ
                .OrderBy(c => c.User.FullName)
                .Take(Math.Clamp(request.Take, 1, 1000))
                .Select(c => new { c.Id, c.User.FullName })
                .ToListAsync(ct);

            if (requiredIds.Count == 0)
            {
                return coaches.Select(c => new CoachDocumentStatusListItemDto
                {
                    CoachId = c.Id,
                    FullName = c.FullName,
                    IsComplete = true,
                    MissingCount = 0
                }).ToList();
            }

            var coachIds = coaches.Select(x => x.Id).ToList();

            var uploadedPairs = await _dataScope.Documents()
                .Where(d => d.IsActive && d.CoachId != null && coachIds.Contains(d.CoachId.Value))
                .Select(d => new { CoachId = d.CoachId!.Value, d.DocumentDefinitionId })
                .Distinct()
                .ToListAsync(ct);

            var uploadedMap = uploadedPairs
                .GroupBy(x => x.CoachId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.DocumentDefinitionId).ToHashSet());

            var result = new List<CoachDocumentStatusListItemDto>(coaches.Count);

            foreach (var c in coaches)
            {
                uploadedMap.TryGetValue(c.Id, out var uploadedSet);
                uploadedSet ??= new HashSet<int>();

                var missingCount = requiredIds.Count(id => !uploadedSet.Contains(id));
                var isComplete = missingCount == 0;

                if (request.OnlyIncomplete && isComplete)
                    continue;

                result.Add(new CoachDocumentStatusListItemDto
                {
                    CoachId = c.Id,
                    FullName = c.FullName,
                    IsComplete = isComplete,
                    MissingCount = missingCount
                });
            }

            return result;
        }
    }
}
