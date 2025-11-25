using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory
{
    public class StudentAttendanceHistoryItemDto
    {
        public int SessionId { get; set; }

        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public AttendanceStatus Status { get; set; }

        public int? Score { get; set; }
        public string? Note { get; set; }
        public string? CoachComment { get; set; }
    }
}
