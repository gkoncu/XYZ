using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Web.Models.Theming;

namespace XYZ.Web.Models.Dashboard
{
    public class AdminDashboardViewModel
    {
        public TenantThemeViewModel Theme { get; set; } = new TenantThemeViewModel();
        public AdminCoachDashboardDto Stats { get; set; } = new AdminCoachDashboardDto();
        public string UserDisplayName { get; set; } = "Kullanıcı";
    }
}
