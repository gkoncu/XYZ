using System;

namespace XYZ.Web.Models.Attendance
{
    public class TodaySessionViewModel
    {
        public int SessionId { get; set; }

        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? BranchName { get; set; }

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }

        public bool HasAttendance { get; set; }
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
    }
}
