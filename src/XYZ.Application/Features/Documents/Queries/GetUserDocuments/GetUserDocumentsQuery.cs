using MediatR;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.GetUserDocuments
{
    public class GetUserDocumentsQuery : IRequest<IList<DocumentListItemDto>>
    {
        public DocumentTarget Target { get; set; }
        public int OwnerId { get; set; }
        public int? DocumentDefinitionId { get; set; }
    }
}
