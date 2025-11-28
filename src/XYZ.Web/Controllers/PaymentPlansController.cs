using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class PaymentPlansController : Controller
    {
        private readonly IApiClient _apiClient;

        public PaymentPlansController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,Student,SuperAdmin")]
        public async Task<IActionResult> Student(
            int studentId,
            CancellationToken cancellationToken)
        {
            var plan = await _apiClient.GetStudentPaymentPlanAsync(studentId, cancellationToken);

            ViewBag.StudentId = studentId;

            if (plan == null)
            {
                return View("StudentPlan", model: null);
            }

            return View("StudentPlan", plan);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult Create(int studentId)
        {
            var today = DateTime.Today;

            var model = new CreatePaymentPlanCommand
            {
                StudentId = studentId,
                TotalAmount = 0,
                TotalInstallments = 1,
                FirstDueDate = today,
                IsInstallment = false
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreatePaymentPlanCommand model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _apiClient.CreatePaymentPlanAsync(model, cancellationToken);

                return RedirectToAction("Student", new { studentId = model.StudentId });
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Ödeme planı oluşturulurken bir hata oluştu.");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> My(CancellationToken cancellationToken)
        {
            var plan = await _apiClient.GetMyPaymentPlanAsync(cancellationToken);

            if (plan == null)
            {
                ViewBag.StudentId = 0;
                return View("StudentPlan", model: null);
            }

            ViewBag.StudentId = plan.StudentId;
            return View("StudentPlan", plan);
        }
    }
}
