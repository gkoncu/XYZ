using XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords;

namespace XYZ.Web.Models.ProgressRecords
{
    public sealed class StudentProgressRecordsViewModel
    {
        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;

        public int? BranchId { get; set; }
        public string? BranchName { get; set; }

        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }

        public List<ProgressRecordListItemDto> Items { get; set; } = new();

        public bool CanWrite { get; set; }

        public List<(int Id, string Name)> BranchOptions { get; set; } = new();
    }
}
