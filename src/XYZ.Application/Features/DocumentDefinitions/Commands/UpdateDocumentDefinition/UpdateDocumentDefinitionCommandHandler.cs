using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.UpdateDocumentDefinition
{
    public class UpdateDocumentDefinitionCommandHandler : IRequestHandler<UpdateDocumentDefinitionCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public UpdateDocumentDefinitionCommandHandler(IApplicationDbContext context, ICurrentUserService current)
        {
            _context = context;
            _current = current;
        }

        public async Task<int> Handle(UpdateDocumentDefinitionCommand request, CancellationToken ct)
        {
            var entity = await _context.DocumentDefinitions.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null) throw new NotFoundException("DocumentDefinition", request.Id);

            if (_current.Role != "SuperAdmin")
            {
                var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");
                if (entity.TenantId != tenantId)
                    throw new UnauthorizedAccessException("Bu belge tanımını güncelleyemezsiniz.");
            }

            entity.Target = request.Target;
            entity.Name = request.Name.Trim();
            entity.IsRequired = request.IsRequired;
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }
    }
}
