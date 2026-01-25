using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Web.Models.Dashboard;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "Coach")]
    public class CoachDashboardController : Controller
    {
        private readonly IApiClient _apiClient;

        public CoachDashboardController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var stats = await _apiClient.GetAdminCoachDashboardAsync(cancellationToken)
                        ?? new AdminCoachDashboardDto();

            var model = new CoachDashboardViewModel
            {
                Stats = stats,
                UserDisplayName = User?.Identity?.Name ?? "Koç"
            };

            return View(model);
        }
    }
}
