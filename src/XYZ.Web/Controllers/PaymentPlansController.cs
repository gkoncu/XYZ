using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan;
using XYZ.Web.Models.PaymentPlans;
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
        public async Task<IActionResult> Student(int studentId, CancellationToken cancellationToken)
        {
            var plan = await _apiClient.GetStudentPaymentPlanAsync(studentId, cancellationToken);

            ViewBag.StudentId = studentId;

            if (plan == null)
                return View("StudentPlan", model: null);

            return View("StudentPlan", plan);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,Student,SuperAdmin")]
        public async Task<IActionResult> History(int studentId, CancellationToken cancellationToken)
        {
            var history = await _apiClient.GetStudentPaymentPlanHistoryAsync(studentId, cancellationToken);
            ViewBag.StudentId = studentId;
            return View(history);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,Student,SuperAdmin")]
        public async Task<IActionResult> Details(int planId, CancellationToken cancellationToken)
        {
            var plan = await _apiClient.GetPaymentPlanDetailsAsync(planId, cancellationToken);
            if (plan == null)
                return NotFound();

            ViewBag.StudentId = plan.StudentId;
            ViewBag.IsPlanDetails = true;

            return View("StudentPlan", plan);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Create(int studentId, CancellationToken cancellationToken)
        {
            var today = DateTime.Today;

            var student = await _apiClient.GetStudentAsync(studentId, cancellationToken);

            var activePlan = await _apiClient.GetStudentPaymentPlanAsync(studentId, cancellationToken);

            ViewBag.ActivePlanExists = activePlan != null;
            ViewBag.ActivePlanUnpaidCount = activePlan?.Installments?.Count(i => i.Status != XYZ.Domain.Enums.PaymentStatus.Paid) ?? 0;

            var model = new CreatePaymentPlanVm
            {
                StudentId = studentId,
                StudentFullName = student?.FullName ?? "",
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
        public async Task<IActionResult> Create(CreatePaymentPlanVm model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var cmd = new CreatePaymentPlanCommand
                {
                    StudentId = model.StudentId,
                    TotalAmount = model.TotalAmount,
                    TotalInstallments = model.TotalInstallments,
                    FirstDueDate = model.FirstDueDate,
                    IsInstallment = model.IsInstallment
                };

                await _apiClient.CreatePaymentPlanAsync(cmd, cancellationToken);

                return RedirectToAction("Student", new { studentId = model.StudentId });
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Aidat planı oluşturulurken bir hata oluştu.");
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

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int planId, int studentId, CancellationToken cancellationToken)
        {
            try
            {
                await _apiClient.CancelPaymentPlanAsync(planId, cancellationToken);
                TempData["SuccessMessage"] = "Aidat planı iptal edildi.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Aidat planı iptal edilirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Student), new { studentId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int planId, int studentId, CancellationToken cancellationToken)
        {
            try
            {
                await _apiClient.ArchivePaymentPlanAsync(planId, cancellationToken);
                TempData["SuccessMessage"] = "Aidat planı arşivlendi.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Aidat planı arşivlenirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(History), new { studentId });
        }
    }
}
