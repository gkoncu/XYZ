using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;
using XYZ.Domain.Enums;

namespace XYZ.Domain.Entities
{
    public class Attendance : BaseEntity
    {
        public int ClassSessionId { get; set; }
        public int ClassId { get; set; }
        public int StudentId { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Unknown;
        public string? Note { get; set; }
        public int? Score { get; set; }
        public string? CoachComment { get; set; }

        public ClassSession ClassSession { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}
