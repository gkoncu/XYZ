using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.Payments.Commands.CreatePayment;
using XYZ.Application.Features.Payments.Commands.UpdatePayment;
using XYZ.Domain.Enums;
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
            ? "Öğrenci Ödemeleri"
            : "Ödemeler";

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
        ViewData["Title"] = "Ödemelerim";

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
    public IActionResult Create(int? studentId)
    {
        var model = new CreatePaymentCommand
        {
            StudentId = studentId ?? 0
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePaymentCommand model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var id = await _apiClient.CreatePaymentAsync(model, cancellationToken);

            if (model.StudentId > 0)
            {
                return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
            }

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Ödeme kaydedilirken bir hata oluştu.");
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

        var model = new UpdatePaymentCommand
        {
            Id = payment.Id,
            Amount = payment.Amount,
            DiscountAmount = payment.DiscountAmount,
            Status = payment.Status
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdatePaymentCommand model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _apiClient.UpdatePaymentAsync(model, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Ödeme güncellenirken bir hata oluştu.");
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
        }
        catch
        {
            TempData["Error"] = "Ödeme silinirken bir hata oluştu.";
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
            TempData["SuccessMessage"] = "Bu ödeme zaten 'Ödendi' durumunda.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (payment.Status == PaymentStatus.Cancelled)
        {
            TempData["ErrorMessage"] = "İptal edilmiş bir ödeme 'Ödendi' olarak işaretlenemez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            var cmd = new UpdatePaymentCommand
            {
                Id = payment.Id,
                Amount = payment.Amount,
                DiscountAmount = payment.DiscountAmount,
                Status = PaymentStatus.Paid
            };

            await _apiClient.UpdatePaymentAsync(cmd, cancellationToken);
            TempData["SuccessMessage"] = "Ödeme 'Ödendi' olarak işaretlendi.";
        }
        catch
        {
            TempData["ErrorMessage"] = "Ödeme durumu güncellenirken bir hata oluştu.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
