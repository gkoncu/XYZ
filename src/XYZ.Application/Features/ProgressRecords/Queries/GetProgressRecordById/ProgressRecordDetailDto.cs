using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetProgressRecordById
{
    public class ProgressRecordDetailDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }

        public DateOnly RecordDate { get; set; }
        public int Sequence { get; set; }

        public string? CreatedByDisplayName { get; set; }

        public string? CoachNotes { get; set; }
        public string? Goals { get; set; }

        public IList<ProgressRecordMetricValueDto> Values { get; set; } = new List<ProgressRecordMetricValueDto>();
    }

    public class ProgressRecordMetricValueDto
    {
        public int ProgressMetricDefinitionId { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public ProgressMetricDataType DataType { get; set; }
        public string? Unit { get; set; }

        public decimal? DecimalValue { get; set; }
        public int? IntValue { get; set; }
        public string? TextValue { get; set; }
    }
}
