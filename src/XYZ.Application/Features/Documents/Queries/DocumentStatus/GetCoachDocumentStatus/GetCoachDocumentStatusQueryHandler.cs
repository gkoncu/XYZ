using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetCoachDocumentStatus
{
    public class GetCoachDocumentStatusQueryHandler
        : IRequestHandler<GetCoachDocumentStatusQuery, UserDocumentStatusDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public GetCoachDocumentStatusQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService current)
        {
            _context = context;
            _dataScope = dataScope;
            _current = current;
        }

        public async Task<UserDocumentStatusDto> Handle(GetCoachDocumentStatusQuery request, CancellationToken ct)
        {
            var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var coach = await _dataScope.Coaches()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == request.CoachId, ct);

            if (coach is null)
                throw new NotFoundException("Coach", request.CoachId);

            var required = await _context.DocumentDefinitions
                .Where(d => d.TenantId == tenantId
                            && d.Target == DocumentTarget.Coach
                            && d.IsActive
                            && d.IsRequired)
                .OrderBy(d => d.SortOrder).ThenBy(d => d.Name)
                .Select(d => new MissingDocumentTypeDto
                {
                    DocumentDefinitionId = d.Id,
                    Name = d.Name,
                    SortOrder = d.SortOrder,
                    IsRequired = d.IsRequired
                })
                .ToListAsync(ct);

            var uploadedIds = await _dataScope.Documents()
                .Where(d => d.IsActive && d.CoachId == request.CoachId)
                .Select(d => d.DocumentDefinitionId)
                .Distinct()
                .ToListAsync(ct);

            var missing = required
                .Where(r => !uploadedIds.Contains(r.DocumentDefinitionId))
                .ToList();

            return new UserDocumentStatusDto
            {
                Target = DocumentTarget.Coach,
                OwnerId = request.CoachId,
                IsComplete = missing.Count == 0,
                MissingCount = missing.Count,
                Missing = missing
            };
        }
    }
}
