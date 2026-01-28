using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Classes.Queries.GetAllClasses;
using XYZ.Application.Features.ClassSessions.Commands.CreateClassSession;
using XYZ.Application.Features.ClassSessions.Commands.UpdateClassSession;
using XYZ.Application.Features.ClassSessions.Queries.GetClassSessionById;
using XYZ.Application.Features.ClassSessions.Queries.GetClassSessions;
using XYZ.Domain.Enums;
using XYZ.Web.Models.ClassSessions;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "Admin,Coach,SuperAdmin")]
    public class ClassSessionsController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<ClassSessionsController> _logger;

        public ClassSessionsController(
            IApiClient apiClient,
            ILogger<ClassSessionsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? classId,
            int? branchId,
            SessionStatus? status,
            DateOnly? from,
            DateOnly? to,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            var effectiveFrom = from ?? DateOnly.FromDateTime(DateTime.Today);
            var effectiveTo = to ?? effectiveFrom;

            ViewBag.From = effectiveFrom.ToString("yyyy-MM-dd");
            ViewBag.To = effectiveTo.ToString("yyyy-MM-dd");
            ViewBag.ClassId = classId;
            ViewBag.BranchId = branchId;
            ViewBag.Status = status?.ToString();

            ViewBag.Classes = await LoadClassesSelectListAsync(selectedClassId: classId, ct: ct);
            ViewBag.Statuses = LoadSessionStatusSelectList(selected: status);

            var emptyModel = new PaginationResult<ClassSessionListItemDto>
            {
                Items = new List<ClassSessionListItemDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };

            try
            {
                var path =
                    $"classsessions?from={effectiveFrom:yyyy-MM-dd}" +
                    $"&to={effectiveTo:yyyy-MM-dd}" +
                    $"&pageNumber={pageNumber}" +
                    $"&pageSize={pageSize}";

                if (classId.HasValue) path += $"&classId={classId.Value}";
                if (branchId.HasValue) path += $"&branchId={branchId.Value}";
                if (status.HasValue) path += $"&status={status.Value}";

                var response = await _apiClient.GetAsync(path, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "classsessions isteği başarısız. StatusCode: {StatusCode}",
                        response.StatusCode);

                    TempData["ErrorMessage"] = "Seanslar yüklenirken bir hata oluştu.";
                    return View(emptyModel);
                }

                var result = await response.Content
                    .ReadFromJsonAsync<PaginationResult<ClassSessionListItemDto>>(cancellationToken: ct);

                if (result is null)
                {
                    TempData["ErrorMessage"] = "Seans verileri çözümlenemedi.";
                    return View(emptyModel);
                }

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seanslar alınırken beklenmeyen hata oluştu.");
                TempData["ErrorMessage"] = "Seanslar yüklenirken beklenmeyen bir hata oluştu.";
                return View(emptyModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _apiClient.GetAsync($"classsessions/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "classsessions/{Id} isteği başarısız. StatusCode: {StatusCode}",
                        id,
                        response.StatusCode);

                    TempData["ErrorMessage"] = "Seans detayı yüklenirken bir hata oluştu.";
                    return RedirectToAction(nameof(Index));
                }

                var dto = await response.Content
                    .ReadFromJsonAsync<ClassSessionDetailDto>(cancellationToken: ct);

                if (dto is null)
                {
                    TempData["ErrorMessage"] = "Seans detayı çözümlenemedi.";
                    return RedirectToAction(nameof(Index));
                }

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seans detayı alınırken beklenmeyen hata oluştu.");
                TempData["ErrorMessage"] = "Seans detayı yüklenirken beklenmeyen bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? classId, DateOnly? date, CancellationToken ct = default)
        {
            ViewBag.Classes = await LoadClassesSelectListAsync(selectedClassId: classId, ct: ct);

            var d = date ?? DateOnly.FromDateTime(DateTime.Today);

            var vm = new CreateClassSessionVm
            {
                ClassId = classId ?? 0,
                Date = d.ToString("yyyy-MM-dd"),
                StartTime = "18:00",
                EndTime = "19:00",
                Title = "Antrenman",
                GenerateAttendance = true
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClassSessionVm vm, CancellationToken ct = default)
        {
            ViewBag.Classes = await LoadClassesSelectListAsync(selectedClassId: vm.ClassId, ct: ct);

            if (!ModelState.IsValid)
                return View(vm);

            if (!TryParseDateTime(vm.Date, vm.StartTime, vm.EndTime, out var date, out var start, out var end))
                return View(vm);

            try
            {
                var cmd = new CreateClassSessionCommand(
                    ClassId: vm.ClassId,
                    Date: date,
                    StartTime: start,
                    EndTime: end,
                    Title: vm.Title,
                    Description: vm.Description,
                    Location: vm.Location,
                    GenerateAttendance: vm.GenerateAttendance
                );

                var response = await _apiClient.PostAsJsonAsync("classsessions", cmd, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Seans oluşturulamadı.";
                    return View(vm);
                }

                var newId = await response.Content.ReadFromJsonAsync<int>(cancellationToken: ct);

                TempData["SuccessMessage"] = "Seans oluşturuldu.";
                return RedirectToAction(nameof(Details), new { id = newId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seans oluşturulurken beklenmeyen hata oluştu.");
                TempData["ErrorMessage"] = "Seans oluşturulurken beklenmeyen bir hata oluştu.";
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _apiClient.GetAsync($"classsessions/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Seans detayı alınamadı.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var dto = await response.Content.ReadFromJsonAsync<ClassSessionDetailDto>(cancellationToken: ct);
                if (dto is null)
                {
                    TempData["ErrorMessage"] = "Seans detayı çözümlenemedi.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var vm = new EditClassSessionVm
                {
                    SessionId = dto.Id,
                    Date = dto.Date.ToString("yyyy-MM-dd"),
                    StartTime = dto.StartTime.ToString("HH:mm"),
                    EndTime = dto.EndTime.ToString("HH:mm"),
                    Title = dto.Title,
                    Description = dto.Description,
                    Location = dto.Location,
                    CoachNote = dto.CoachNote
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seans düzenleme sayfası açılırken hata oluştu.");
                TempData["ErrorMessage"] = "Seans düzenleme sayfası yüklenemedi.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditClassSessionVm vm, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (!TryParseDateTime(vm.Date, vm.StartTime, vm.EndTime, out var date, out var start, out var end))
                return View(vm);

            try
            {
                var cmd = new UpdateClassSessionCommand
                {
                    SessionId = vm.SessionId,
                    Date = date,
                    StartTime = start,
                    EndTime = end,
                    Title = vm.Title,
                    Description = vm.Description,
                    Location = vm.Location,
                    CoachNote = vm.CoachNote
                };

                var response = await _apiClient.PutAsJsonAsync($"classsessions/{vm.SessionId}", cmd, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Seans güncellenemedi.";
                    return View(vm);
                }

                TempData["SuccessMessage"] = "Seans güncellendi.";
                return RedirectToAction(nameof(Details), new { id = vm.SessionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seans güncellenirken beklenmeyen hata oluştu.");
                TempData["ErrorMessage"] = "Seans güncellenirken beklenmeyen bir hata oluştu.";
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _apiClient.DeleteAsync($"classsessions/{id}", ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Seans silinemedi.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["SuccessMessage"] = "Seans silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seans silinirken hata oluştu.");
                TempData["ErrorMessage"] = "Seans silinirken beklenmeyen bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> BulkCreate(int classId, CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var vm = new BulkCreateClassSessionsVm
            {
                ClassId = classId,
                FromDate = today.ToString("yyyy-MM-dd"),
                ToDate = today.AddDays(30).ToString("yyyy-MM-dd"),
                StartTime = "18:00",
                EndTime = "19:00",
                Title = "Antrenman",
                GenerateAttendance = true,
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true
            };

            ViewBag.Classes = await LoadClassesSelectListAsync(selectedClassId: classId, ct: ct);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(BulkCreateClassSessionsVm vm, CancellationToken ct = default)
        {
            ViewBag.Classes = await LoadClassesSelectListAsync(selectedClassId: vm.ClassId, ct: ct);

            if (!ModelState.IsValid)
                return View(vm);

            if (!TryParseBulk(vm, out var fromDate, out var toDate, out var start, out var end, out var daySet))
                return View(vm);

            try
            {
                var existing = await GetExistingSessionsAsync(vm.ClassId, fromDate, toDate, ct);
                var existingKeys = existing
                    .Select(x => $"{x.Date:yyyy-MM-dd}|{x.StartTime:HH\\:mm}")
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                int created = 0;
                int skipped = 0;

                for (var d = fromDate; d <= toDate; d = d.AddDays(1))
                {
                    if (!daySet.Contains(d.DayOfWeek))
                        continue;

                    var key = $"{d:yyyy-MM-dd}|{start:HH\\:mm}";
                    if (existingKeys.Contains(key))
                    {
                        skipped++;
                        continue;
                    }

                    var cmd = new CreateClassSessionCommand(
                        ClassId: vm.ClassId,
                        Date: d,
                        StartTime: start,
                        EndTime: end,
                        Title: vm.Title,
                        Description: vm.Description,
                        Location: vm.Location,
                        GenerateAttendance: vm.GenerateAttendance
                    );

                    var resp = await _apiClient.PostAsJsonAsync("classsessions", cmd, ct);

                    if (resp.StatusCode == HttpStatusCode.Unauthorized)
                        return RedirectToAction("Login", "Account");

                    if (!resp.IsSuccessStatusCode)
                    {
                        TempData["ErrorMessage"] = $"Program oluşturma sırasında hata oluştu. Oluşan: {created}, Atlanan: {skipped}.";
                        return View(vm);
                    }

                    created++;
                }

                TempData["SuccessMessage"] = $"Program oluşturuldu. Oluşan: {created}, Atlanan: {skipped}.";
                return RedirectToAction(nameof(Index),
                    new
                    {
                        classId = vm.ClassId,
                        from = fromDate.ToString("yyyy-MM-dd"),
                        to = toDate.ToString("yyyy-MM-dd")
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk seans oluşturma sırasında hata oluştu.");
                TempData["ErrorMessage"] = "Program oluşturulurken beklenmeyen bir hata oluştu.";
                return View(vm);
            }
        }
---
        private static bool TryParseDateTime(
            string dateStr,
            string startStr,
            string endStr,
            out DateOnly date,
            out TimeOnly start,
            out TimeOnly end)
        {
            date = default;
            start = default;
            end = default;

            if (!DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return Fail("Tarih formatı hatalı. Örn: 2026-01-28");

            if (!TimeOnly.TryParseExact(startStr, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
                return Fail("Başlangıç saati formatı hatalı. Örn: 18:00");

            if (!TimeOnly.TryParseExact(endStr, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
                return Fail("Bitiş saati formatı hatalı. Örn: 19:00");

            if (end <= start)
                return Fail("Bitiş saati, başlangıç saatinden büyük olmalı.");

            return true;

            static bool Fail(string _)
            {
                return false;
            }
        }

        private bool TryParseBulk(
            BulkCreateClassSessionsVm vm,
            out DateOnly fromDate,
            out DateOnly toDate,
            out TimeOnly start,
            out TimeOnly end,
            out HashSet<DayOfWeek> daySet)
        {
            fromDate = default;
            toDate = default;
            start = default;
            end = default;
            daySet = new HashSet<DayOfWeek>();

            if (!DateOnly.TryParseExact(vm.FromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate))
            {
                ModelState.AddModelError(nameof(vm.FromDate), "Başlangıç tarihi formatı hatalı. Örn: 2026-01-28");
                return false;
            }

            if (!DateOnly.TryParseExact(vm.ToDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
            {
                ModelState.AddModelError(nameof(vm.ToDate), "Bitiş tarihi formatı hatalı. Örn: 2026-02-28");
                return false;
            }

            if (toDate < fromDate)
            {
                ModelState.AddModelError(nameof(vm.ToDate), "Bitiş tarihi başlangıçtan küçük olamaz.");
                return false;
            }

            if (!TimeOnly.TryParseExact(vm.StartTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
            {
                ModelState.AddModelError(nameof(vm.StartTime), "Başlangıç saati formatı hatalı. Örn: 18:00");
                return false;
            }

            if (!TimeOnly.TryParseExact(vm.EndTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
            {
                ModelState.AddModelError(nameof(vm.EndTime), "Bitiş saati formatı hatalı. Örn: 19:00");
                return false;
            }

            if (end <= start)
            {
                ModelState.AddModelError(nameof(vm.EndTime), "Bitiş saati başlangıçtan büyük olmalı.");
                return false;
            }

            if (vm.Monday) daySet.Add(DayOfWeek.Monday);
            if (vm.Tuesday) daySet.Add(DayOfWeek.Tuesday);
            if (vm.Wednesday) daySet.Add(DayOfWeek.Wednesday);
            if (vm.Thursday) daySet.Add(DayOfWeek.Thursday);
            if (vm.Friday) daySet.Add(DayOfWeek.Friday);
            if (vm.Saturday) daySet.Add(DayOfWeek.Saturday);
            if (vm.Sunday) daySet.Add(DayOfWeek.Sunday);

            if (daySet.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "En az 1 gün seçmelisin.");
                return false;
            }

            return true;
        }

        private async Task<List<SelectListItem>> LoadClassesSelectListAsync(int? selectedClassId, CancellationToken ct)
        {
            try
            {
                var response = await _apiClient.GetAsync("classes?pageNumber=1&pageSize=500", ct);
                if (!response.IsSuccessStatusCode)
                    return new List<SelectListItem>();

                var result = await response.Content.ReadFromJsonAsync<PaginationResult<ClassListItemDto>>(cancellationToken: ct);
                if (result?.Items is null)
                    return new List<SelectListItem>();

                return result.Items
                    .Select(c => new SelectListItem(c.Name, c.Id.ToString(), selectedClassId == c.Id))
                    .ToList();
            }
            catch
            {
                return new List<SelectListItem>();
            }
        }

        private static List<SelectListItem> LoadSessionStatusSelectList(SessionStatus? selected)
        {
            var items = new List<SelectListItem>
            {
                new("Tümü", "", selected is null)
            };

            foreach (var val in Enum.GetValues<SessionStatus>())
            {
                items.Add(new SelectListItem(val.ToString(), val.ToString(), selected == val));
            }

            return items;
        }

        private async Task<List<ClassSessionListItemDto>> GetExistingSessionsAsync(int classId, DateOnly from, DateOnly to, CancellationToken ct)
        {
            var response = await _apiClient.GetAsync(
                $"classsessions?classId={classId}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&pageNumber=1&pageSize=2000",
                ct);

            if (!response.IsSuccessStatusCode)
                return new List<ClassSessionListItemDto>();

            var result = await response.Content.ReadFromJsonAsync<PaginationResult<ClassSessionListItemDto>>(cancellationToken: ct);
            return result?.Items?.ToList() ?? new List<ClassSessionListItemDto>();
        }
    }
}
