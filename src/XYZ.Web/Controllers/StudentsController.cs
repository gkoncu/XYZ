using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;




using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
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
            var result = await _apiClient.GetStudentsAsync(searchTerm, pageNumber, pageSize, cancellationToken);
            ViewBag.SearchTerm = searchTerm;

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var student = await _apiClient.GetStudentAsync(id, cancellationToken);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }
    }
}