using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.Payments.Commands.CreatePayment;
using XYZ.Application.Features.Payments.Commands.UpdatePayment;
using XYZ.Domain.Enums;
using XYZ.Web.Extensions;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers;

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
        DateOnly? fromDueDate,
        DateOnly? toDueDate,
        PaymentStatus? status,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!fromDueDate.HasValue && !toDueDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            fromDueDate = today.AddDays(-30);
            toDueDate = today.AddDays(30);
        }

        var result = await _apiClient.GetPaymentsAsync(
            studentId: studentId,
            fromDueDate: fromDueDate,
            toDueDate: toDueDate,
            status: status,
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        ViewBag.StudentId = studentId;
        ViewBag.IsMyPayments = false;
        ViewBag.FromDueDate = fromDueDate;
        ViewBag.ToDueDate = toDueDate;
        ViewBag.Status = status;

        ViewData["Title"] = studentId.HasValue
            ? "Öğrenci Aidatları"
            : "Aidatlar";

        return View(result);
    }

    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> My(
        DateOnly? fromDueDate,
        DateOnly? toDueDate,
        PaymentStatus? status,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!fromDueDate.HasValue && !toDueDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            fromDueDate = today.AddDays(-30);
            toDueDate = today.AddDays(30);
        }

        var result = await _apiClient.GetPaymentsAsync(
            studentId: null,
            fromDueDate: fromDueDate,
            toDueDate: toDueDate,
            status: status,
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        ViewBag.StudentId = null;
        ViewBag.IsMyPayments = true;
        ViewBag.FromDueDate = fromDueDate;
        ViewBag.ToDueDate = toDueDate;
        ViewBag.Status = status;
        ViewData["Title"] = "Aidatlarım";

        return View("Index", result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Coach,Student,SuperAdmin")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var payment = await _apiClient.GetPaymentAsync(id, cancellationToken);
        if (payment == null)
        {
            return NotFound();
        }

        return View(payment);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create(int? studentId, CancellationToken cancellationToken = default)
    {
        if (!studentId.HasValue || studentId.Value <= 0)
        {
            TempData["ErrorMessage"] = "Aidat oluşturmak için önce bir öğrenci seçmelisiniz.";
            return RedirectToAction("Index", "Students");
        }

        var student = await _apiClient.GetStudentAsync(studentId.Value, cancellationToken);
        if (student == null)
        {
            TempData["ErrorMessage"] = "Öğrenci bulunamadı.";
            return RedirectToAction("Index", "Students");
        }

        ViewBag.StudentFullName = student.FullName;

        var model = new CreatePaymentCommand
        {
            StudentId = studentId.Value,
            DueDate = DateTime.Today,
            Status = PaymentStatus.Pending
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePaymentCommand model, CancellationToken cancellationToken)
    {
        if (model.StudentId > 0)
        {
            var student = await _apiClient.GetStudentAsync(model.StudentId, cancellationToken);
            ViewBag.StudentFullName = student?.FullName;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var id = await _apiClient.CreatePaymentAsync(model, cancellationToken);
            return RedirectToAction(nameof(Details), new { id });
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Aidat kaydedilirken bir hata oluştu.");
            return View(model);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var payment = await _apiClient.GetPaymentAsync(id, cancellationToken);
        if (payment == null)
        {
            return NotFound();
        }

        ViewBag.StudentFullName = payment.StudentFullName;

        var model = new UpdatePaymentCommand
        {
            Id = payment.Id,
            Amount = payment.Amount,
            DiscountAmount = payment.DiscountAmount,
            Status = payment.Status,
            Notes = payment.Notes
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdatePaymentCommand model, CancellationToken cancellationToken)
    {
        var payment = await _apiClient.GetPaymentAsync(model.Id, cancellationToken);
        ViewBag.StudentFullName = payment?.StudentFullName;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _apiClient.UpdatePaymentAsync(model, cancellationToken);
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Aidat güncellenirken bir hata oluştu.");
            return View(model);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? studentId, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeletePaymentAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Aidat silindi.";
        }
        catch
        {
            TempData["ErrorMessage"] = "Aidat silinirken bir hata oluştu.";
        }

        if (studentId.HasValue && studentId.Value > 0)
        {
            return RedirectToAction(nameof(Index), new { studentId = studentId.Value });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(int id, CancellationToken cancellationToken)
    {
        var payment = await _apiClient.GetPaymentAsync(id, cancellationToken);
        if (payment == null)
        {
            return NotFound();
        }

        if (payment.Status == PaymentStatus.Paid)
        {
            TempData["SuccessMessage"] = "Bu aidat zaten 'Ödendi' durumunda.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (payment.Status == PaymentStatus.Cancelled)
        {
            TempData["ErrorMessage"] = "İptal edilmiş bir aidat 'Ödendi' olarak işaretlenemez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            var cmd = new UpdatePaymentCommand
            {
                Id = payment.Id,
                Amount = payment.Amount,
                DiscountAmount = payment.DiscountAmount,
                Status = PaymentStatus.Paid,
                Notes = payment.Notes
            };

            await _apiClient.UpdatePaymentAsync(cmd, cancellationToken);
            TempData["SuccessMessage"] = "Aidat 'Ödendi' olarak işaretlendi.";
        }
        catch
        {
            TempData["ErrorMessage"] = "Aidat durumu güncellenirken bir hata oluştu.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
