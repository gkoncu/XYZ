using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Web.Models.Theming;

namespace XYZ.Web.Models.Dashboard
{
    public class AdminDashboardViewModel
    {
        public AdminCoachDashboardDto Stats { get; set; } = new();
        public string UserDisplayName { get; set; } = string.Empty;
    }
}
