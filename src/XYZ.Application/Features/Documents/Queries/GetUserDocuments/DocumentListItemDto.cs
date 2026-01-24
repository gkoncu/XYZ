using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.GetUserDocuments
{
    public class DocumentListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; } = string.Empty;

        public int DocumentDefinitionId { get; set; }
        public string DocumentDefinitionName { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public DocumentTarget Target { get; set; }
    }
}
