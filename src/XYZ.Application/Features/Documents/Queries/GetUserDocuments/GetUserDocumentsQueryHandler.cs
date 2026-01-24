using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.GetUserDocuments
{
    public class GetUserDocumentsQueryHandler : IRequestHandler<GetUserDocumentsQuery, IList<DocumentListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetUserDocumentsQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<IList<DocumentListItemDto>> Handle(GetUserDocumentsQuery request, CancellationToken cancellationToken)
        {
            var q = _dataScope.Documents()
                .Include(d => d.DocumentDefinition)
                .AsQueryable();

            q = request.Target switch
            {
                DocumentTarget.Student => q.Where(d => d.StudentId == request.OwnerId),
                DocumentTarget.Coach => q.Where(d => d.CoachId == request.OwnerId),
                _ => q.Where(_ => false)
            };

            if (request.DocumentDefinitionId.HasValue)
                q = q.Where(d => d.DocumentDefinitionId == request.DocumentDefinitionId.Value);

            return await q
                .OrderByDescending(d => d.UploadDate)
                .Select(d => new DocumentListItemDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    UploadDate = d.UploadDate,
                    UploadedBy = d.UploadedBy,
                    DocumentDefinitionId = d.DocumentDefinitionId,
                    DocumentDefinitionName = d.DocumentDefinition.Name,
                    IsRequired = d.DocumentDefinition.IsRequired,
                    Target = d.DocumentDefinition.Target
                })
                .ToListAsync(cancellationToken);
        }
    }
}
