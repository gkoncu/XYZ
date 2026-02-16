using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard;
using XYZ.Domain.Constants;
using XYZ.Web.Infrastructure;
using XYZ.Web.Models.Dashboard;
using XYZ.Web.Models.Theming;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = RoleNames.Student)]
    public class StudentDashboardController : Controller
    {
        private readonly IApiClient _apiClient;

        public StudentDashboardController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var stats = await _apiClient.GetStudentDashboardAsync(cancellationToken)
                        ?? new StudentDashboardDto();

            var model = new StudentDashboardViewModel
            {
                Stats = stats,
                UserDisplayName = User?.Identity?.Name ?? "Öğrenci"
            };

            return View(model);
        }
    }
}
