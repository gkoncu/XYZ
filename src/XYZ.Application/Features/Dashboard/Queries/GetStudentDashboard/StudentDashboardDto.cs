using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard
{
    public class StudentDashboardDto
    {
        public int TotalSessions { get; set; }
        public int AttendedSessions { get; set; }
        public int MissedSessions { get; set; }

        public int UpcomingSessionsCount { get; set; }

        public DateOnly? NextSessionDate { get; set; }
        public TimeOnly? NextSessionStartTime { get; set; }
        public string? NextSessionClassName { get; set; }

        public int OutstandingPaymentsCount { get; set; }
        public decimal OutstandingPaymentsAmount { get; set; }
    }
}
