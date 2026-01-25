using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;

namespace XYZ.Web.Models.Dashboard
{
    public class CoachDashboardViewModel
    {
        public string UserDisplayName { get; set; } = "Koç";
        public AdminCoachDashboardDto Stats { get; set; } = new();
    }
}
