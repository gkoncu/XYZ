using MediatR;

namespace XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitionById
{
    public class GetDocumentDefinitionByIdQuery : IRequest<DocumentDefinitionDetailDto>
    {
        public int Id { get; set; }
    }
}
