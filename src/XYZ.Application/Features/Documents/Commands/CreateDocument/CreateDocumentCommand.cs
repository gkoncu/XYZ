using MediatR;

namespace XYZ.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommand : IRequest<int>
    {
        public int DocumentDefinitionId { get; set; }

        public int? StudentId { get; set; }
        public int? CoachId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
