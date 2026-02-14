using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitionById
{
    public sealed class GetDocumentDefinitionByIdQueryHandler
        : IRequestHandler<GetDocumentDefinitionByIdQuery, DocumentDefinitionDetailDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public GetDocumentDefinitionByIdQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService current)
        {
            _context = context;
            _current = current;
        }

        public async Task<DocumentDefinitionDetailDto> Handle(
            GetDocumentDefinitionByIdQuery request,
            CancellationToken ct)
        {
            var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var entity = await _context.DocumentDefinitions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.TenantId == tenantId, ct);

            if (entity is null) throw new NotFoundException("DocumentDefinition", request.Id);

            return new DocumentDefinitionDetailDto
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                Target = entity.Target,
                Name = entity.Name,
                IsRequired = entity.IsRequired,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };
        }
    }
}