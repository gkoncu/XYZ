namespace XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord
{
    public class MetricValueInput
    {
        public int ProgressMetricDefinitionId { get; set; }
        public decimal? DecimalValue { get; set; }
        public int? IntValue { get; set; }
        public string? TextValue { get; set; }
    }
}
