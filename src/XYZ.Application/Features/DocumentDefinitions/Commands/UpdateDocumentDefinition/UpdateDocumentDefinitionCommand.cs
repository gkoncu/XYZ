using MediatR;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.UpdateDocumentDefinition
{
    public class UpdateDocumentDefinitionCommand : IRequest<int>
    {
        public int Id { get; set; }
        public DocumentTarget Target { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
