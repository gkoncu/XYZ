using System;
using System.Collections.Generic;
using XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory;

namespace XYZ.Web.Models.Attendance
{
    public class StudentAttendanceHistoryViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;

        public DateOnly From { get; set; }
        public DateOnly To { get; set; }

        public IList<StudentAttendanceHistoryItemDto> Items { get; set; }
            = new List<StudentAttendanceHistoryItemDto>();
    }
}
