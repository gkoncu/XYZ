using MediatR;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.DeleteDocumentDefinition
{
    public class DeleteDocumentDefinitionCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}
