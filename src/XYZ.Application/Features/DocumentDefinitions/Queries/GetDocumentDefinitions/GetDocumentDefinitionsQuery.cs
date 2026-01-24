using MediatR;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitions
{
    public class GetDocumentDefinitionsQuery : IRequest<IList<DocumentDefinitionListItemDto>>
    {
        public DocumentTarget? Target { get; set; }
        public bool IncludeInactive { get; set; }
    }
}
