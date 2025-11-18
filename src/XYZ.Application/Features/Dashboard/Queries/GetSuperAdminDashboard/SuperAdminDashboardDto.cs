using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard
{
    public class SuperAdminDashboardDto
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }

        public int TotalStudents { get; set; }
        public int TotalCoaches { get; set; }
        public int TotalClasses { get; set; }

        public int TotalPayments { get; set; }
        public decimal TotalPaidAmount { get; set; }

        public int UpcomingSessions { get; set; }
    }
}
