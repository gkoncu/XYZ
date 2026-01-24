using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus
{
    public class UserDocumentStatusDto
    {
        public DocumentTarget Target { get; set; }
        public int OwnerId { get; set; }

        public bool IsComplete { get; set; }
        public int MissingCount { get; set; }

        public IList<MissingDocumentTypeDto> Missing { get; set; } = new List<MissingDocumentTypeDto>();
    }

    public class MissingDocumentTypeDto
    {
        public int DocumentDefinitionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class StudentDocumentStatusListItemDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;

        public bool IsComplete { get; set; }
        public int MissingCount { get; set; }
    }

    public class CoachDocumentStatusListItemDto
    {
        public int CoachId { get; set; }
        public string FullName { get; set; } = string.Empty;

        public bool IsComplete { get; set; }
        public int MissingCount { get; set; }
    }
}
