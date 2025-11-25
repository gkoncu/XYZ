using System;

namespace XYZ.Application.Features.Attendances.Queries.GetAttendanceOverview
{
    public class AttendanceOverviewDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;

        public DateOnly From { get; set; }
        public DateOnly To { get; set; }

        public int TotalSessions { get; set; }
        public int TotalAttendanceRecords { get; set; }

        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }
        public int LateCount { get; set; }
        public int UnknownCount { get; set; }
        public double AttendanceRate { get; set; }
    }
}
