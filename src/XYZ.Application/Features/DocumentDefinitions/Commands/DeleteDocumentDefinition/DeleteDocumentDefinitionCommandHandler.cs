using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.DeleteDocumentDefinition
{
    public class DeleteDocumentDefinitionCommandHandler : IRequestHandler<DeleteDocumentDefinitionCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public DeleteDocumentDefinitionCommandHandler(IApplicationDbContext context, ICurrentUserService current)
        {
            _context = context;
            _current = current;
        }

        public async Task<int> Handle(DeleteDocumentDefinitionCommand request, CancellationToken ct)
        {
            var entity = await _context.DocumentDefinitions.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null) throw new NotFoundException("DocumentDefinition", request.Id);

            if (_current.Role != "SuperAdmin")
            {
                var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");
                if (entity.TenantId != tenantId)
                    throw new UnauthorizedAccessException("Bu belge tanımını silemezsiniz.");
            }

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }
    }
}
