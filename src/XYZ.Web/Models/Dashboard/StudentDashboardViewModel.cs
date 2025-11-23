using XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard;
using XYZ.Web.Models.Theming;

namespace XYZ.Web.Models.Dashboard
{
    public class StudentDashboardViewModel
    {
        public TenantThemeViewModel Theme { get; set; } = new();
        public StudentDashboardDto Stats { get; set; } = new();
        public string UserDisplayName { get; set; } = string.Empty;
    }
}
