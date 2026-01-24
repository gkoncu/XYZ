using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitionById
{
    public class GetDocumentDefinitionByIdQueryHandler
        : IRequestHandler<GetDocumentDefinitionByIdQuery, DocumentDefinitionDetailDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public GetDocumentDefinitionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService current)
        {
            _context = context;
            _current = current;
        }

        public async Task<DocumentDefinitionDetailDto> Handle(GetDocumentDefinitionByIdQuery request, CancellationToken ct)
        {
            var entity = await _context.DocumentDefinitions.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null) throw new NotFoundException("DocumentDefinition", request.Id);

            if (_current.Role != "SuperAdmin")
            {
                var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");
                if (entity.TenantId != tenantId)
                    throw new UnauthorizedAccessException("Bu belge tanımına erişiminiz yok.");
            }

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
