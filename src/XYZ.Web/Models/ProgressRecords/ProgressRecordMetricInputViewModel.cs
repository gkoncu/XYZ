using XYZ.Domain.Enums;

namespace XYZ.Web.Models.ProgressRecords
{
    public sealed class ProgressRecordMetricInputViewModel
    {
        public int ProgressMetricDefinitionId { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public ProgressMetricDataType DataType { get; set; }
        public string? Unit { get; set; }

        public bool IsRequired { get; set; }

        public decimal? DecimalValue { get; set; }
        public int? IntValue { get; set; }
        public string? TextValue { get; set; }
    }
}
