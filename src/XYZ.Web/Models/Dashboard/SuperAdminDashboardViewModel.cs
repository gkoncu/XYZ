using XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard;
using XYZ.Web.Models.Theming;

namespace XYZ.Web.Models.Dashboard
{
    public class SuperAdminDashboardViewModel
    {
        public TenantThemeViewModel Theme { get; set; } = new();
        public SuperAdminDashboardDto Stats { get; set; } = new();
        public string UserDisplayName { get; set; } = string.Empty;
    }
}
