using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XYZ.Web.Models.Dashboard;
using XYZ.Web.Models.Theming;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly IApiClient _apiClient;

        public AdminDashboardController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var stats = await _apiClient.GetAdminCoachDashboardAsync(cancellationToken)
                        ?? new XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard.AdminCoachDashboardDto();

            // TODO: Sabiti değiştir ve temayı dinamik yap
            var theme = new TenantThemeViewModel
            {
                Name = "XYZ Sports Club",
                PrimaryColor = "#3B82F6",
                SecondaryColor = "#1E40AF",
                LogoUrl = null
            };

            var model = new AdminDashboardViewModel
            {
                Theme = theme,
                Stats = stats,
                UserDisplayName = User?.Identity?.Name ?? "Kullanıcı"
            };

            ViewData["PrimaryColor"] = theme.PrimaryColor;
            ViewData["SecondaryColor"] = theme.SecondaryColor;
            ViewData["TenantName"] = theme.Name;
            ViewData["LogoUrl"] = theme.LogoUrl;

            return View(model);
        }
    }
}
