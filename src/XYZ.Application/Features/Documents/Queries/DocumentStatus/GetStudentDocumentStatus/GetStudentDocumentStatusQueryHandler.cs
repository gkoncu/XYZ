using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetStudentDocumentStatus
{
    public class GetStudentDocumentStatusQueryHandler
        : IRequestHandler<GetStudentDocumentStatusQuery, UserDocumentStatusDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public GetStudentDocumentStatusQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService current)
        {
            _context = context;
            _dataScope = dataScope;
            _current = current;
        }

        public async Task<UserDocumentStatusDto> Handle(GetStudentDocumentStatusQuery request, CancellationToken ct)
        {
            var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var student = await _dataScope.Students()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student is null)
                throw new NotFoundException("Student", request.StudentId);

            var required = await _context.DocumentDefinitions
                .Where(d => d.TenantId == tenantId
                            && d.Target == DocumentTarget.Student
                            && d.IsActive
                            && d.IsRequired)
                .OrderBy(d => d.SortOrder).ThenBy(d => d.Name)
                .Select(d => new MissingDocumentTypeDto
                {
                    DocumentDefinitionId = d.Id,
                    Name = d.Name,
                    SortOrder = d.SortOrder,
                    IsRequired = d.IsRequired
                })
                .ToListAsync(ct);

            var uploadedIds = await _dataScope.Documents()
                .Where(d => d.IsActive && d.StudentId == request.StudentId)
                .Select(d => d.DocumentDefinitionId)
                .Distinct()
                .ToListAsync(ct);

            var missing = required
                .Where(r => !uploadedIds.Contains(r.DocumentDefinitionId))
                .ToList();

            return new UserDocumentStatusDto
            {
                Target = DocumentTarget.Student,
                OwnerId = request.StudentId,
                IsComplete = missing.Count == 0,
                MissingCount = missing.Count,
                Missing = missing
            };
        }
    }
}
