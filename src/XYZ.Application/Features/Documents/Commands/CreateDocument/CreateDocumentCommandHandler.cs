using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, int>
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
            if (role is null || role is not (RoleNames.Admin or RoleNames.Coach or RoleNames.SuperAdmin or RoleNames.Student))
                throw new UnauthorizedAccessException("Doküman yükleme yetkiniz yok.");

            var definition = await _context.DocumentDefinitions
                .FirstOrDefaultAsync(dd => dd.Id == request.DocumentDefinitionId, ct);

            if (definition is null)
                throw new NotFoundException("DocumentDefinition", request.DocumentDefinitionId);

            if (definition.Target == DocumentTarget.Student)
            {
                var studentId = request.StudentId ?? 0;
                if (studentId <= 0)
                    throw new InvalidOperationException("Student belgeleri için StudentId zorunludur.");

                if (role == RoleNames.Student && _current.StudentId.HasValue && _current.StudentId.Value != studentId)
                    throw new UnauthorizedAccessException("Sadece kendi belgelerinizi yükleyebilirsiniz.");

                var student = await _dataScope.Students()
                    .FirstOrDefaultAsync(s => s.Id == studentId, ct);

                if (student is null)
                    throw new NotFoundException(RoleNames.Student, studentId);

                if (definition.TenantId != student.TenantId)
                    throw new UnauthorizedAccessException("Belge tipi bu öğrenciyle aynı kulübe ait değil.");

                var now = DateTime.UtcNow;

                var entity = new Document
                {
                    TenantId = student.TenantId,
                    StudentId = student.Id,
                    DocumentDefinitionId = definition.Id,
                    Name = request.Name.Trim(),
                    FilePath = request.FilePath.Trim(),
                    Description = request.Description,
                    UploadDate = now,
                    UploadedBy = _current.UserId ?? string.Empty,
                    IsActive = true,
                    CreatedAt = now
                };

                await _context.Documents.AddAsync(entity, ct);
                await _context.SaveChangesAsync(ct);
                return entity.Id;
            }

            {
                var coachId = request.CoachId ?? 0;
                if (coachId <= 0)
                    throw new InvalidOperationException("Coach belgeleri için CoachId zorunludur.");

                if (role == RoleNames.Coach && _current.CoachId.HasValue && _current.CoachId.Value != coachId)
                    throw new UnauthorizedAccessException("Sadece kendi belgelerinizi yükleyebilirsiniz.");

                var coach = await _dataScope.Coaches()
                    .FirstOrDefaultAsync(c => c.Id == coachId, ct);

                if (coach is null)
                    throw new NotFoundException("Coach", coachId);

                if (definition.TenantId != coach.TenantId)
                    throw new UnauthorizedAccessException("Belge tipi bu koçla aynı kulübe ait değil.");

                var now = DateTime.UtcNow;

                var entity = new Document
                {
                    TenantId = coach.TenantId,
                    CoachId = coach.Id,
                    DocumentDefinitionId = definition.Id,
                    Name = request.Name.Trim(),
                    FilePath = request.FilePath.Trim(),
                    Description = request.Description,
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
}
