using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard;
using XYZ.Web.Infrastructure;
using XYZ.Web.Models.Dashboard;
using XYZ.Web.Models.Theming;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminDashboardController : Controller
    {
        private readonly IApiClient _apiClient;

        public SuperAdminDashboardController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var stats = await _apiClient.GetSuperAdminDashboardAsync(cancellationToken)
                        ?? new SuperAdminDashboardDto();

            var theme = TenantThemeFilter.GetThemeFromHttpContext(HttpContext)
                        ?? new TenantThemeViewModel();

            var model = new SuperAdminDashboardViewModel
            {
                Theme = theme,
                Stats = stats,
                UserDisplayName = User?.Identity?.Name ?? "Super Admin"
            };

            return View(model);
        }
    }
}
