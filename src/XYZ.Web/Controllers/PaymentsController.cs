using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly IApiClient _apiClient;

        public PaymentsController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<IActionResult> Index(
            int? studentId,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _apiClient.GetPaymentsAsync(
                studentId,
                pageNumber,
                pageSize,
                cancellationToken);

            ViewBag.StudentId = studentId;
            ViewBag.IsMyPayments = false;

            ViewData["Title"] = studentId.HasValue
                ? "Öğrenci Ödemeleri"
                : "Ödemeler";

            return View(result);
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> My(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _apiClient.GetPaymentsAsync(
                studentId: null,
                pageNumber: pageNumber,
                pageSize: pageSize,
                cancellationToken: cancellationToken);

            ViewBag.StudentId = null;
            ViewBag.IsMyPayments = true;
            ViewData["Title"] = "Ödemelerim";

            return View("Index", result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,Student,SuperAdmin")]
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken)
        {
            var payment = await _apiClient.GetPaymentAsync(id, cancellationToken);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }
    }
}
