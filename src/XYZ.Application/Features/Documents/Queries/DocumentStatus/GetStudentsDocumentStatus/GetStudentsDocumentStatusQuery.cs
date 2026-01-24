using MediatR;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetStudentsDocumentStatus
{
    public class GetStudentsDocumentStatusQuery : IRequest<IList<StudentDocumentStatusListItemDto>>
    {
        public bool OnlyIncomplete { get; set; }
        public string? SearchTerm { get; set; }
        public int Take { get; set; } = 200;
    }
}
