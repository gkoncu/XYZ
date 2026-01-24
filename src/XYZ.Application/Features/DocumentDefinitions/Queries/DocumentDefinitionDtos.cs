using XYZ.Domain.Enums;

namespace XYZ.Application.Features.DocumentDefinitions.Queries
{
    public class DocumentDefinitionListItemDto
    {
        public int Id { get; set; }
        public DocumentTarget Target { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class DocumentDefinitionDetailDto : DocumentDefinitionListItemDto
    {
        public int TenantId { get; set; }
    }
}
