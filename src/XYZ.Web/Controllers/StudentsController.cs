using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IApiClient _apiClient;

        public StudentsController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            // TODO: Sabiti değiştir ve temayı dinamik yap
            ViewData["PrimaryColor"] = "#3B82F6";
            ViewData["SecondaryColor"] = "#1E40AF";
            ViewData["TenantName"] = "XYZ Sports Club";

            var result = await _apiClient.GetStudentsAsync(searchTerm, pageNumber, pageSize, cancellationToken);
            ViewBag.SearchTerm = searchTerm;

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            ViewData["PrimaryColor"] = "#3B82F6";
            ViewData["SecondaryColor"] = "#1E40AF";
            ViewData["TenantName"] = "XYZ Sports Club";

            var student = await _apiClient.GetStudentAsync(id, cancellationToken);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }
    }
}
