using XYZ.Domain.Common;
using XYZ.Domain.Enums;

namespace XYZ.Domain.Entities
{
    public class ProgressMetricDefinition : TenantScopedEntity
    {
        public int BranchId { get; set; }

        public string Name { get; set; } = string.Empty;
        public ProgressMetricDataType DataType { get; set; }
        public string? Unit { get; set; }

        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }

        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }

        public Branch Branch { get; set; } = null!;
    }
}
