using System;
using System.Collections.Generic;
using XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords;

namespace XYZ.Web.Models.ProgressRecords
{
    public sealed class StudentProgressRecordsViewModel
    {
        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public List<ProgressRecordListItemDto> Items { get; set; } = new();

        public bool CanWrite { get; set; }
    }
}
