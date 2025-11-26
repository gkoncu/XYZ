using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Attendances.Queries.GetAttendanceList
{
    public class AttendanceListItemDto
    {
        public int Id { get; set; }

        public int ClassSessionId { get; set; }
        public DateOnly SessionDate { get; set; }

        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;

        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;

        public AttendanceStatus Status { get; set; }

        public int? Score { get; set; }
        public string? Note { get; set; }
        public string? CoachComment { get; set; }
    }
}
