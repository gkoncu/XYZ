using MediatR;

namespace XYZ.Application.Features.Documents.Commands.UpdateDocument
{
    public class UpdateDocumentCommand : IRequest<int>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string? FilePath { get; set; }

        public bool? IsActive { get; set; }
    }
}
