using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.CreateDocumentDefinition
{
    public class CreateDocumentDefinitionCommandHandler : IRequestHandler<CreateDocumentDefinitionCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public CreateDocumentDefinitionCommandHandler(IApplicationDbContext context, ICurrentUserService current)
        {
            _context = context;
            _current = current;
        }

        public async Task<int> Handle(CreateDocumentDefinitionCommand request, CancellationToken ct)
        {
            var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var entity = new DocumentDefinition
            {
                TenantId = tenantId,
                Target = request.Target,
                Name = request.Name.Trim(),
                IsRequired = request.IsRequired,
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _context.DocumentDefinitions.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }
    }
}
