namespace XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord
{
    public class MetricValueInput
    {
        public int ProgressMetricDefinitionId { get; set; }
        public decimal? DecimalValue { get; set; }
        public int? IntValue { get; set; }
        public string? TextValue { get; set; }
    }
}
