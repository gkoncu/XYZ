using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XYZ.Web.Models.Attendance;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "Admin,Coach,SuperAdmin")]
    public class AttendanceController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            IApiClient apiClient,
            ILogger<AttendanceController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? date, CancellationToken ct)
        {
            try
            {
                var path = "attendances/today-sessions";

                if (!string.IsNullOrWhiteSpace(date))
                {
                    path += $"?date={WebUtility.UrlEncode(date)}";
                }

                var response = await _apiClient.GetAsync(path, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("today-sessions isteği başarısız. StatusCode: {StatusCode}",
                        response.StatusCode);
                    ViewData["ErrorMessage"] = "Seanslar yüklenirken bir hata oluştu.";
                    ViewBag.SelectedDate = date;
                    return View(new List<TodaySessionViewModel>());
                }

                var sessions = await response.Content
                    .ReadFromJsonAsync<IList<TodaySessionViewModel>>(cancellationToken: ct)
                    ?? new List<TodaySessionViewModel>();

                ViewBag.SelectedDate = date;
                return View(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seanslar alınırken beklenmeyen hata oluştu.");
                ViewData["ErrorMessage"] = "Seanslar yüklenirken beklenmeyen bir hata oluştu.";
                ViewBag.SelectedDate = date;
                return View(new List<TodaySessionViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Session(int id, CancellationToken ct)
        {
            try
            {
                var response = await _apiClient.GetAsync(
                    $"attendances/session/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Session {SessionId} için attendance yüklenemedi. StatusCode: {StatusCode}",
                        id, response.StatusCode);

                    TempData["ErrorMessage"] = "Yoklama bilgileri yüklenirken bir hata oluştu.";
                    return RedirectToAction(nameof(Index));
                }

                var dto = await response.Content
                    .ReadFromJsonAsync<SessionAttendanceDto>(cancellationToken: ct);

                if (dto is null)
                {
                    TempData["ErrorMessage"] = "Yoklama bilgileri yüklenemedi.";
                    return RedirectToAction(nameof(Index));
                }

                var vm = new SessionAttendanceViewModel
                {
                    SessionId = dto.SessionId,
                    ClassId = dto.ClassId,
                    ClassName = dto.ClassName,
                    Date = dto.Date,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Title = dto.Title,
                    Location = dto.Location,
                    Status = dto.Status,
                    Students = dto.Students.Select(s => new SessionAttendanceStudentItem
                    {
                        AttendanceId = s.AttendanceId,
                        StudentId = s.StudentId,
                        FullName = s.FullName,
                        Status = s.Status,
                        Note = s.Note,
                        Score = s.Score,
                        CoachComment = s.CoachComment
                    }).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Session {SessionId} için attendance alınırken beklenmeyen hata.",
                    id);

                TempData["ErrorMessage"] = "Yoklama bilgileri yüklenirken beklenmeyen bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Session(SessionAttendanceViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var payload = new
            {
                SessionId = model.SessionId,
                Items = model.Students.Select(s => new
                {
                    AttendanceId = s.AttendanceId,
                    Status = s.Status,
                    Note = s.Note,
                    Score = s.Score,
                    CoachComment = s.CoachComment
                }).ToList()
            };

            try
            {
                var response = await _apiClient.PutAsJsonAsync(
                    $"attendances/session/{model.SessionId}",
                    payload,
                    ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Session {SessionId} yoklama güncellemesi başarısız. StatusCode: {StatusCode}",
                        model.SessionId,
                        response.StatusCode);

                    ModelState.AddModelError(string.Empty, "Yoklama kaydedilirken bir hata oluştu.");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Yoklama başarıyla güncellendi.";
                return RedirectToAction(nameof(Session), new { id = model.SessionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Session {SessionId} yoklama güncellemesinde beklenmeyen hata.",
                    model.SessionId);

                ModelState.AddModelError(string.Empty, "Yoklama kaydedilirken beklenmeyen bir hata oluştu.");
                return View(model);
            }
        }
    }
}
