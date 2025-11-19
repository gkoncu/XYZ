using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQueryHandler
        : IRequestHandler<GetDocumentByIdQuery, DocumentDetailDto>
    {
        private readonly IDataScopeService _dataScope;

        public GetDocumentByIdQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<DocumentDetailDto> Handle(
            GetDocumentByIdQuery request,
            CancellationToken ct)
        {
            var entity = await _dataScope.Documents()
                .Include(d => d.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(d => d.Id == request.Id, ct);

            if (entity is null)
                throw new NotFoundException("Document", request.Id);

            return new DocumentDetailDto
            {
                Id = entity.Id,
                StudentId = entity.StudentId ?? 0,
                StudentFullName = entity.Student?.User.FullName ?? string.Empty,
                Name = entity.Name,
                FilePath = entity.FilePath,
                Description = entity.Description,
                Type = entity.Type,
                UploadDate = entity.UploadDate,
                UploadedBy = entity.UploadedBy,
                IsActive = entity.IsActive
            };
        }
    }
}
