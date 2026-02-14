using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommandHandler
        : IRequestHandler<DeleteDocumentCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly IFileService _fileService;

        public DeleteDocumentCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            IFileService fileService)
        {
            _dataScope = dataScope;
            _context = context;
            _fileService = fileService;
        }

        public async Task<int> Handle(DeleteDocumentCommand request, CancellationToken ct)
        {
            var entity = await _dataScope.Documents()
                .FirstOrDefaultAsync(d => d.Id == request.Id, ct);

            if (entity is null)
                throw new NotFoundException("Document", request.Id);

            if (!string.IsNullOrWhiteSpace(entity.FilePath))
                await _fileService.DeleteFileAsync(entity.FilePath);

            _context.Documents.Remove(entity);
            await _context.SaveChangesAsync(ct);

            return request.Id;
        }
    }
}
