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
        public int StudentId { get; set; }
        public int ClassScheduleId { get; set; }
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Notes { get; set; }

        public Student Student { get; set; } = null!;
        public ClassSchedule ClassSchedule { get; set; } = null!;
    }
}
