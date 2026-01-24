using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.GetDocumentById
{
    public class DocumentDetailDto
    {
        public int Id { get; set; }

        public int? StudentId { get; set; }
        public string? StudentFullName { get; set; }

        public int? CoachId { get; set; }
        public string? CoachFullName { get; set; }

        public int DocumentDefinitionId { get; set; }
        public string DocumentDefinitionName { get; set; } = string.Empty;
        public DocumentTarget Target { get; set; }
        public bool IsRequired { get; set; }

        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
