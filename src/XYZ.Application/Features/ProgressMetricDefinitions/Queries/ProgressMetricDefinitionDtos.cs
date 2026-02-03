using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Queries
{
    public class ProgressMetricDefinitionListItemDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int BranchId { get; set; }

        public string Name { get; set; } = string.Empty;
        public ProgressMetricDataType DataType { get; set; }
        public string? Unit { get; set; }

        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }

        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }

        public bool IsActive { get; set; }
    }

    public class ProgressMetricDefinitionDetailDto : ProgressMetricDefinitionListItemDto
    {
        public string? BranchName { get; set; }
    }
}