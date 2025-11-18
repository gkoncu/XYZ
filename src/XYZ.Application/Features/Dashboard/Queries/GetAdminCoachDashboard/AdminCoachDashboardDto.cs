using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard
{
    public class AdminCoachDashboardDto
    {
        public int StudentCount { get; set; }
        public int ClassCount { get; set; }

        public int UpcomingSessionsCount { get; set; }
        public int TodaySessionsCount { get; set; }

        public int PendingPaymentsCount { get; set; }
        public decimal PendingPaymentsAmount { get; set; }

        public int RecentAnnouncementsCount { get; set; }
    }
}
