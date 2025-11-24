using System;
using System.Collections.Generic;

namespace XYZ.Web.Models.Attendance
{
    public class SessionAttendanceViewModel
    {
        public int SessionId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Location { get; set; }
        public int Status { get; set; }

        public IList<SessionAttendanceStudentItem> Students { get; set; }
            = new List<SessionAttendanceStudentItem>();
    }

    public class SessionAttendanceStudentItem
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int Status { get; set; }

        public string? Note { get; set; }
        public int? Score { get; set; }
        public string? CoachComment { get; set; }
    }
}
