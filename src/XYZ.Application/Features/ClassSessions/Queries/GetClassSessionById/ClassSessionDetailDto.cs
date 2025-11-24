using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Queries.GetClassSessionById
{
    public class ClassSessionDetailDto
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
        public string? Description { get; set; }
        public string? Location { get; set; }

        public SessionStatus Status { get; set; }
        public string? CoachNote { get; set; }

        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
    }
}
