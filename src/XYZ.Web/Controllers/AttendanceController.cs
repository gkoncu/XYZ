using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Attendances.Queries.GetAttendanceList;
using XYZ.Web.Models.Attendance;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
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
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [Obsolete("Use ClassSession Detail Screen for Navigate to Attendance")]
        public async Task<IActionResult> Index(string? date, CancellationToken ct)
        {
            var d = DateOnly.TryParse(date, out var parsed)
                ? parsed
                : DateOnly.FromDateTime(DateTime.Today);

            var dStr = d.ToString("yyyy-MM-dd");

            return RedirectToAction(
                actionName: "Index",
                controllerName: "ClassSessions",
                routeValues: new { from = dStr, to = dStr });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> List(
            int? studentId,
            int? classId,
            string? from,
            string? to,
            int? status,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken ct = default)
        {
            var vm = new AttendanceListViewModel
            {
                Filter = new AttendanceListFilter
                {
                    StudentId = studentId,
                    ClassId = classId,
                    From = from,
                    To = to,
                    Status = status,
                    PageNumber = pageNumber <= 0 ? 1 : pageNumber,
                    PageSize = pageSize <= 0 ? 50 : pageSize
                }
            };

            try
            {
                if (User.IsInRole("Student"))
                {
                    vm.Filter.StudentId = null;
                    vm.Filter.ClassId = null;
                }

                var path = BuildAttendanceListPath(vm.Filter);

                var response = await _apiClient.GetAsync(path, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "Bu yoklama kayıtlarına erişim yetkiniz yok.";
                    return RedirectToAction("Index", "Home");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("attendances/list isteği başarısız. StatusCode: {StatusCode}", response.StatusCode);
                    ViewData["ErrorMessage"] = "Yoklama listesi yüklenirken bir hata oluştu.";

                    return View(vm);
                }

                var dto = await response.Content
                    .ReadFromJsonAsync<PaginationResult<AttendanceListItemDto>>(cancellationToken: ct);

                if (dto is null)
                {
                    ViewData["ErrorMessage"] = "Yoklama listesi yüklenirken bir hata oluştu.";
                    return View(vm);
                }

                vm.Result = dto;
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yoklama listesi alınırken beklenmeyen hata oluştu.");
                ViewData["ErrorMessage"] = "Yoklama listesi yüklenirken beklenmeyen bir hata oluştu.";
                return View(vm);
            }
        }

        private static string BuildAttendanceListPath(AttendanceListFilter f)
        {
            var path = "attendances/list";

            var q = new Dictionary<string, string?>();

            if (f.StudentId.HasValue) q["StudentId"] = f.StudentId.Value.ToString();
            if (f.ClassId.HasValue) q["ClassId"] = f.ClassId.Value.ToString();

            if (!string.IsNullOrWhiteSpace(f.From)) q["From"] = f.From;
            if (!string.IsNullOrWhiteSpace(f.To)) q["To"] = f.To;

            if (f.Status.HasValue) q["Status"] = f.Status.Value.ToString();

            q["PageNumber"] = f.PageNumber.ToString();
            q["PageSize"] = f.PageSize.ToString();

            return QueryHelpers.AddQueryString(path, q);
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<IActionResult> Session(int id, CancellationToken ct)
        {
            try
            {
                var response = await _apiClient.GetAsync(
                    $"attendances/session/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

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
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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
                    return RedirectToAction("Login", "Account");

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
