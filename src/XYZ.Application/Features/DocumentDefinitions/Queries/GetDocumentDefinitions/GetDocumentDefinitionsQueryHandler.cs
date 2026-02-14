using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitions
{
    public sealed class GetDocumentDefinitionsQueryHandler
        : IRequestHandler<GetDocumentDefinitionsQuery, IList<DocumentDefinitionListItemDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public GetDocumentDefinitionsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService current)
        {
            _context = context;
            _current = current;
        }

        public async Task<IList<DocumentDefinitionListItemDto>> Handle(
            GetDocumentDefinitionsQuery request,
            CancellationToken ct)
        {
            var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var q = _context.DocumentDefinitions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId);

            if (!request.IncludeInactive)
                q = q.Where(x => x.IsActive);

            if (request.Target.HasValue)
                q = q.Where(x => x.Target == request.Target.Value);

            return await q
                .OrderBy(x => x.Target)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new DocumentDefinitionListItemDto
                {
                    Id = x.Id,
                    Target = x.Target,
                    Name = x.Name,
                    IsRequired = x.IsRequired,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }
    }
}