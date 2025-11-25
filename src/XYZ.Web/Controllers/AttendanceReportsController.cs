using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XYZ.Application.Features.Attendances.Queries.GetAttendanceOverview;
using XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory;
using XYZ.Web.Models.Attendance;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "Admin,Coach,SuperAdmin")]
    public class AttendanceReportsController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<AttendanceReportsController> _logger;

        public AttendanceReportsController(
            IApiClient apiClient,
            ILogger<AttendanceReportsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> StudentHistory(
            int studentId,
            string? studentName,
            DateOnly? from,
            DateOnly? to,
            CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var defaultFrom = today.AddDays(-30);

            var effectiveFrom = from ?? defaultFrom;
            var effectiveTo = to ?? today;

            var model = new StudentAttendanceHistoryViewModel
            {
                StudentId = studentId,
                StudentName = string.IsNullOrWhiteSpace(studentName)
                    ? $"Öğrenci #{studentId}"
                    : studentName,
                From = effectiveFrom,
                To = effectiveTo
            };

            try
            {
                var path =
                    $"attendances/by-student" +
                    $"?studentId={studentId}" +
                    $"&from={effectiveFrom:yyyy-MM-dd}" +
                    $"&to={effectiveTo:yyyy-MM-dd}";

                var response = await _apiClient.GetAsync(path, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "StudentHistory isteği başarısız. StatusCode: {StatusCode}, StudentId: {StudentId}",
                        response.StatusCode,
                        studentId);

                    TempData["ErrorMessage"] = "Öğrenci yoklama geçmişi yüklenirken bir hata oluştu.";
                    return View(model);
                }

                var items = await response.Content
                    .ReadFromJsonAsync<IList<StudentAttendanceHistoryItemDto>>(cancellationToken: ct);

                model.Items = items ?? new List<StudentAttendanceHistoryItemDto>();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "StudentHistory isteğinde beklenmeyen hata. StudentId: {StudentId}",
                    studentId);

                TempData["ErrorMessage"] = "Öğrenci yoklama geçmişi yüklenirken beklenmeyen bir hata oluştu.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ClassOverview(
            int classId,
            string? className,
            DateOnly? from,
            DateOnly? to,
            CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var defaultFrom = today.AddDays(-30);

            var effectiveFrom = from ?? defaultFrom;
            var effectiveTo = to ?? today;

            var model = new ClassAttendanceOverviewViewModel
            {
                ClassId = classId,
                ClassName = string.IsNullOrWhiteSpace(className)
                    ? $"Sınıf #{classId}"
                    : className,
                From = effectiveFrom,
                To = effectiveTo
            };

            try
            {
                var path =
                    $"attendances/overview" +
                    $"?classId={classId}" +
                    $"&from={effectiveFrom:yyyy-MM-dd}" +
                    $"&to={effectiveTo:yyyy-MM-dd}";

                var response = await _apiClient.GetAsync(path, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "ClassOverview isteği başarısız. StatusCode: {StatusCode}, ClassId: {ClassId}",
                        response.StatusCode,
                        classId);

                    TempData["ErrorMessage"] = "Sınıf yoklama özeti yüklenirken bir hata oluştu.";
                    return View(model);
                }

                var dto = await response.Content
                    .ReadFromJsonAsync<AttendanceOverviewDto>(cancellationToken: ct);

                if (dto is not null)
                {
                    model.ClassName = string.IsNullOrWhiteSpace(className)
                        ? dto.ClassName
                        : className;

                    model.From = dto.From;
                    model.To = dto.To;
                    model.TotalSessions = dto.TotalSessions;
                    model.TotalAttendanceRecords = dto.TotalAttendanceRecords;
                    model.PresentCount = dto.PresentCount;
                    model.AbsentCount = dto.AbsentCount;
                    model.ExcusedCount = dto.ExcusedCount;
                    model.LateCount = dto.LateCount;
                    model.UnknownCount = dto.UnknownCount;
                    model.AttendanceRate = dto.AttendanceRate;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ClassOverview isteğinde beklenmeyen hata. ClassId: {ClassId}",
                    classId);

                TempData["ErrorMessage"] = "Sınıf yoklama özeti yüklenirken beklenmeyen bir hata oluştu.";
                return View(model);
            }
        }
    }
}
