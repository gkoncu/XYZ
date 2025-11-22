using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Web.Infrastructure;
using XYZ.Web.Models.Dashboard;
using XYZ.Web.Models.Theming;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
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

            var theme = TenantThemeFilter.GetThemeFromHttpContext(HttpContext)
                ?? new TenantThemeViewModel();

            var model = new AdminDashboardViewModel
            {
                Theme = theme,
                Stats = stats,
                UserDisplayName = User?.Identity?.Name ?? "Kullanıcı"
            };

            return View(model);
        }
    }
}
