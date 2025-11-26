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
        public async Task<IActionResult> List(
            int? studentId,
            int? classId,
            int? classSessionId,
            string? from,
            string? to,
            int? status,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken ct = default)
        {
            try
            {
                var path = "attendances/list";

                var queryParams = new Dictionary<string, string?>();

                if (studentId.HasValue)
                {
                    queryParams["StudentId"] = studentId.Value.ToString();
                }

                if (classId.HasValue)
                {
                    queryParams["ClassId"] = classId.Value.ToString();
                }

                if (classSessionId.HasValue)
                {
                    queryParams["ClassSessionId"] = classSessionId.Value.ToString();
                }

                if (!string.IsNullOrWhiteSpace(from))
                {
                    queryParams["From"] = from;
                }

                if (!string.IsNullOrWhiteSpace(to))
                {
                    queryParams["To"] = to;
                }

                if (status.HasValue)
                {
                    queryParams["Status"] = status.Value.ToString();
                }

                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 50;

                queryParams["PageNumber"] = pageNumber.ToString();
                queryParams["PageSize"] = pageSize.ToString();

                if (queryParams.Any())
                {
                    path = QueryHelpers.AddQueryString(path, queryParams!);
                }

                var response = await _apiClient.GetAsync(path, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "attendances/list isteği başarısız. StatusCode: {StatusCode}",
                        response.StatusCode);

                    ViewData["ErrorMessage"] = "Yoklama listesi yüklenirken bir hata oluştu.";

                    ViewBag.StudentId = studentId;
                    ViewBag.ClassId = classId;
                    ViewBag.ClassSessionId = classSessionId;
                    ViewBag.From = from;
                    ViewBag.To = to;
                    ViewBag.Status = status;

                    var empty = new PaginationResult<AttendanceListItemDto>
                    {
                        Items = new List<AttendanceListItemDto>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };

                    return View(empty);
                }

                var dto = await response.Content
                    .ReadFromJsonAsync<PaginationResult<AttendanceListItemDto>>(cancellationToken: ct);

                if (dto is null)
                {
                    ViewData["ErrorMessage"] = "Yoklama listesi yüklenirken bir hata oluştu.";

                    ViewBag.StudentId = studentId;
                    ViewBag.ClassId = classId;
                    ViewBag.ClassSessionId = classSessionId;
                    ViewBag.From = from;
                    ViewBag.To = to;
                    ViewBag.Status = status;

                    var empty = new PaginationResult<AttendanceListItemDto>
                    {
                        Items = new List<AttendanceListItemDto>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };

                    return View(empty);
                }

                ViewBag.StudentId = studentId;
                ViewBag.ClassId = classId;
                ViewBag.ClassSessionId = classSessionId;
                ViewBag.From = from;
                ViewBag.To = to;
                ViewBag.Status = status;

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yoklama listesi alınırken beklenmeyen hata oluştu.");

                ViewData["ErrorMessage"] = "Yoklama listesi yüklenirken beklenmeyen bir hata oluştu.";

                ViewBag.StudentId = studentId;
                ViewBag.ClassId = classId;
                ViewBag.ClassSessionId = classSessionId;
                ViewBag.From = from;
                ViewBag.To = to;
                ViewBag.Status = status;

                var empty = new PaginationResult<AttendanceListItemDto>
                {
                    Items = new List<AttendanceListItemDto>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };

                return View(empty);
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
