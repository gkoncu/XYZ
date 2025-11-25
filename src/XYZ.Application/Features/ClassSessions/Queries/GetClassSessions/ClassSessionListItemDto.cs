using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Queries.GetClassSessions
{
    public class ClassSessionListItemDto
    {
        public int Id { get; set; }

        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;

        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string Title { get; set; } = string.Empty;
        public SessionStatus Status { get; set; }

        public bool IsActive { get; set; }

        public bool HasAttendance { get; set; }

        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
    }
}
