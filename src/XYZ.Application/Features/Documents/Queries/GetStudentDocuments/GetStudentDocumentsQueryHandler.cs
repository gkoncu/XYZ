using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Documents.Queries.GetStudentDocuments
{
    public class GetStudentDocumentsQueryHandler
        : IRequestHandler<GetStudentDocumentsQuery, IList<DocumentListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetStudentDocumentsQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<IList<DocumentListItemDto>> Handle(
            GetStudentDocumentsQuery request,
            CancellationToken ct)
        {
            await _dataScope.EnsureStudentAccessAsync(request.StudentId, ct);

            var query = _dataScope.Documents()
                .Where(d => d.StudentId == request.StudentId);

            if (request.Type.HasValue)
            {
                query = query.Where(d => d.Type == request.Type.Value);
            }

            var list = await query
                .OrderByDescending(d => d.UploadDate)
                .ThenByDescending(d => d.Id)
                .Select(d => new DocumentListItemDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    Type = d.Type,
                    UploadDate = d.UploadDate,
                    IsActive = d.IsActive
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return list;
        }
    }
}
