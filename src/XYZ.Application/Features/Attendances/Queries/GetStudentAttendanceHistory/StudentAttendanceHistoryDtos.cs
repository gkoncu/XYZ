using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory
{
    public class StudentAttendanceHistoryItemDto
    {
        public int AttendanceId { get; set; }

        public int ClassSessionId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Title { get; set; } = string.Empty;

        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
        public int? Score { get; set; }
        public string? CoachComment { get; set; }
    }
}
