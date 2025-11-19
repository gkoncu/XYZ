using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommandHandler
        : IRequestHandler<DeleteDocumentCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;
        private readonly IFileService _fileService;

        public DeleteDocumentCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IFileService fileService)
        {
            _dataScope = dataScope;
            _context = context;
            _current = currentUser;
            _fileService = fileService;
        }

        public async Task<int> Handle(DeleteDocumentCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Doküman silme yetkiniz yok.");

            var entity = await _dataScope.Documents()
                .FirstOrDefaultAsync(d => d.Id == request.Id, ct);

            if (entity is null)
                throw new NotFoundException("Document", request.Id);

            if (!string.IsNullOrWhiteSpace(entity.FilePath))
            {
                await _fileService.DeleteFileAsync(entity.FilePath);
            }

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }
    }
}
