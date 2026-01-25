using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Web.Infrastructure;
using XYZ.Web.Models.Dashboard;
using XYZ.Web.Models.Theming;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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
                        ?? new AdminCoachDashboardDto();

            var model = new AdminDashboardViewModel
            {
                Stats = stats,
                UserDisplayName = User?.Identity?.Name ?? "Kullanıcı"
            };

            return View(model);
        }
    }
}
