using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommandHandler
        : IRequestHandler<CreateDocumentCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public CreateDocumentCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
        }

        public async Task<int> Handle(CreateDocumentCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Doküman yükleme yetkiniz yok.");

            var student = await _dataScope.Students()
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student is null)
                throw new NotFoundException("Student", request.StudentId);

            var now = DateTime.UtcNow;

            var entity = new Document
            {
                StudentId = student.Id,
                Name = request.Name.Trim(),
                FilePath = request.FilePath.Trim(),
                Description = request.Description,
                Type = request.Type,
                UploadDate = now,
                UploadedBy = _current.UserId ?? string.Empty,
                IsActive = true,
                CreatedAt = now
            };

            await _context.Documents.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return entity.Id;
        }
    }
}
