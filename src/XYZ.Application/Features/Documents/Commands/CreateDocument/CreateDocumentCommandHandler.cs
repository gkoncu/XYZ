using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

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
            var definition = await _context.DocumentDefinitions
                .FirstOrDefaultAsync(dd => dd.Id == request.DocumentDefinitionId, ct);

            if (definition is null)
                throw new NotFoundException("DocumentDefinition", request.DocumentDefinitionId);

            int? studentId = null;
            int? coachId = null;

            if (definition.Target == DocumentTarget.Student)
            {
                var sid = request.StudentId ?? 0;
                if (sid <= 0)
                    throw new InvalidOperationException("Student belgeleri için StudentId zorunludur.");

                var student = await _dataScope.Students()
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == sid, ct);

                if (student is null)
                    throw new NotFoundException(RoleNames.Student, sid);

                if (definition.TenantId != student.TenantId)
                    throw new InvalidOperationException("Belge tanımı farklı bir tenant'a ait.");

                studentId = sid;
            }
            else
            {
                var cid = request.CoachId ?? 0;
                if (cid <= 0)
                    throw new InvalidOperationException("Coach belgeleri için CoachId zorunludur.");

                var coach = await _dataScope.Coaches()
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == cid, ct);

                if (coach is null)
                    throw new NotFoundException(RoleNames.Coach, cid);

                if (definition.TenantId != coach.TenantId)
                    throw new InvalidOperationException("Belge tanımı farklı bir tenant'a ait.");

                coachId = cid;
            }

            var entity = new Document
            {
                TenantId = definition.TenantId,
                DocumentDefinitionId = definition.Id,
                StudentId = studentId,
                CoachId = coachId,
                Name = string.IsNullOrWhiteSpace(request.Name) ? definition.Name : request.Name!.Trim(),
                Description = request.Description?.Trim(),
                FilePath = request.FilePath.Trim(),
                UploadDate = DateTime.UtcNow,
                UploadedBy = _current.UserId ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Documents.Add(entity);
            await _context.SaveChangesAsync(ct);

            return entity.Id;
        }
    }
}
