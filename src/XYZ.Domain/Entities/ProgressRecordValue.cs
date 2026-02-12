using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class ProgressRecordValue : TenantScopedEntity
    {
        public int ProgressRecordId { get; set; }
        public int ProgressMetricDefinitionId { get; set; }

        public decimal? DecimalValue { get; set; }
        public int? IntValue { get; set; }
        public string? TextValue { get; set; }

        public ProgressRecord ProgressRecord { get; set; } = null!;
        public ProgressMetricDefinition ProgressMetricDefinition { get; set; } = null!;
    }
}
