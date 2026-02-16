using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Documents.Commands.UpdateDocument
{
    public class UpdateDocumentCommandHandler
        : IRequestHandler<UpdateDocumentCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;

        public UpdateDocumentCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context)
        {
            _dataScope = dataScope;
            _context = context;
        }

        public async Task<int> Handle(UpdateDocumentCommand request, CancellationToken ct)
        {
            var entity = await _dataScope.Documents()
                .FirstOrDefaultAsync(d => d.Id == request.Id, ct);

            if (entity is null)
                throw new NotFoundException("Document", request.Id);

            if (request.Name is not null) entity.Name = request.Name.Trim();
            if (request.Description is not null) entity.Description = request.Description.Trim();
            if (request.FilePath is not null) entity.FilePath = request.FilePath.Trim();
            if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;

            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return entity.Id;
        }
    }
}
