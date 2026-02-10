using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.ClassSessions.Commands.CreateClassSession;
using XYZ.Application.Features.ClassSessions.Commands.UpdateClassSession;
using XYZ.Application.Features.ClassSessions.Queries.GetClassSessionById;
using XYZ.Application.Features.ClassSessions.Queries.GetClassSessions;
using XYZ.Domain.Enums;
using XYZ.Web.Models.ClassSessions;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class ClassSessionsController : Controller
    {
        private readonly IApiClient _api;

        public ClassSessionsController(IApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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

            var emptyModel = new PaginationResult<ClassSessionListItemDto>
            {
                Items = new List<ClassSessionListItemDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };

            try
            {
                var query = new Dictionary<string, string?>
                {
                    ["From"] = effectiveFrom.ToString("yyyy-MM-dd"),
                    ["To"] = effectiveTo.ToString("yyyy-MM-dd"),
                    ["PageNumber"] = pageNumber.ToString(),
                    ["PageSize"] = pageSize.ToString(),
                    ["OnlyActive"] = "true",
                    ["SortBy"] = "Date",
                    ["SortDir"] = "asc"
                };

                if (classId.HasValue) query["ClassId"] = classId.Value.ToString();
                if (branchId.HasValue) query["BranchId"] = branchId.Value.ToString();
                if (status.HasValue) query["Status"] = status.Value.ToString();

                var url = QueryHelpers.AddQueryString("classsessions", query);

                var response = await _api.GetAsync(url, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Antrenmanlar yüklenirken bir hata oluştu.";
                    return View(emptyModel);
                }

                var result = await response.Content
                    .ReadFromJsonAsync<PaginationResult<ClassSessionListItemDto>>(cancellationToken: ct);

                return View(result ?? emptyModel);
            }
            catch
            {
                TempData["ErrorMessage"] = "Antrenmanlar yüklenirken beklenmeyen bir hata oluştu.";
                return View(emptyModel);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> My(
            DateOnly? from,
            DateOnly? to,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            var effectiveFrom = from ?? DateOnly.FromDateTime(DateTime.Today);
            var effectiveTo = to ?? effectiveFrom.AddDays(7);

            ViewBag.From = effectiveFrom.ToString("yyyy-MM-dd");
            ViewBag.To = effectiveTo.ToString("yyyy-MM-dd");

            var emptyModel = new PaginationResult<ClassSessionListItemDto>
            {
                Items = new List<ClassSessionListItemDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };

            try
            {
                var query = new Dictionary<string, string?>
                {
                    ["From"] = effectiveFrom.ToString("yyyy-MM-dd"),
                    ["To"] = effectiveTo.ToString("yyyy-MM-dd"),
                    ["PageNumber"] = pageNumber.ToString(),
                    ["PageSize"] = pageSize.ToString(),
                    ["OnlyActive"] = "true",
                    ["SortBy"] = "Date",
                    ["SortDir"] = "asc"
                };

                var url = QueryHelpers.AddQueryString("classsessions/my", query);

                var response = await _api.GetAsync(url, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Ders programı yüklenirken bir hata oluştu.";
                    return View("My", emptyModel);
                }

                var result = await response.Content
                    .ReadFromJsonAsync<PaginationResult<ClassSessionListItemDto>>(cancellationToken: ct);

                return View("My", result ?? emptyModel);
            }
            catch
            {
                TempData["ErrorMessage"] = "Ders programı yüklenirken beklenmeyen bir hata oluştu.";
                return View("My", emptyModel);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> Details(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _api.GetAsync($"classsessions/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Ders detayı yüklenirken bir hata oluştu.";
                    return RedirectToAction(User.IsInRole("Student") ? nameof(My) : nameof(Index));
                }

                var dto = await response.Content
                    .ReadFromJsonAsync<ClassSessionDetailDto>(cancellationToken: ct);

                if (dto is null)
                {
                    TempData["ErrorMessage"] = "Ders detayı çözümlenemedi.";
                    return RedirectToAction(User.IsInRole("Student") ? nameof(My) : nameof(Index));
                }

                return View(dto);
            }
            catch
            {
                TempData["ErrorMessage"] = "Ders detayı yüklenirken beklenmeyen bir hata oluştu.";
                return RedirectToAction(User.IsInRole("Student") ? nameof(My) : nameof(Index));
            }
        }

        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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

        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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

                var response = await _api.PostAsJsonAsync("classsessions", cmd, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Seans oluşturulamadı.";
                    return View(vm);
                }

                var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: ct);

                TempData["SuccessMessage"] = "Seans oluşturuldu.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch
            {
                TempData["ErrorMessage"] = "Seans oluşturulurken beklenmeyen bir hata oluştu.";
                return View(vm);
            }
        }

        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _api.GetAsync($"classsessions/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Seans düzenleme sayfası yüklenemedi.";
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
                    StartTime = dto.StartTime.ToString("HH\\:mm"),
                    EndTime = dto.EndTime.ToString("HH\\:mm"),
                    Title = dto.Title,
                    Description = dto.Description,
                    Location = dto.Location,
                    CoachNote = dto.CoachNote
                };

                return View(vm);
            }
            catch
            {
                TempData["ErrorMessage"] = "Seans düzenleme sayfası yüklenemedi.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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

                var response = await _api.PutAsJsonAsync($"classsessions/{vm.SessionId}", cmd, ct);

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
            catch
            {
                TempData["ErrorMessage"] = "Seans güncellenirken beklenmeyen bir hata oluştu.";
                return View(vm);
            }
        }

        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _api.DeleteAsync($"classsessions/{id}", ct);

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
            catch
            {
                TempData["ErrorMessage"] = "Seans silinirken beklenmeyen bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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

        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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

                var created = 0;
                var skipped = 0;

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

                    var resp = await _api.PostAsJsonAsync("classsessions", cmd, ct);
                    if (resp.IsSuccessStatusCode) created++;
                }

                TempData["SuccessMessage"] = $"Toplu oluşturma tamamlandı. Oluşturulan: {created}, Atlanan: {skipped}.";
                return RedirectToAction(nameof(Index), new { classId = vm.ClassId, from = fromDate, to = toDate });
            }
            catch
            {
                TempData["ErrorMessage"] = "Toplu seans oluşturma sırasında hata oluştu.";
                return View(vm);
            }
        }

        private async Task<List<SelectListItem>> LoadClassesSelectListAsync(int? selectedClassId, CancellationToken ct)
        {
            var result = await _api.GetClassesAsync(searchTerm: null, branchId: null, isActive: true, pageNumber: 1, pageSize: 200, ct);

            return result.Items
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), selectedClassId == c.Id))
                .ToList();
        }

        private bool TryParseDateTime(string dateStr, string startStr, string endStr, out DateOnly date, out TimeOnly start, out TimeOnly end)
        {
            date = default;
            start = default;
            end = default;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var minDate = today.AddYears(-1);
            var maxDate = today.AddDays(364);

            if (!DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                ModelState.AddModelError(nameof(CreateClassSessionVm.Date), "Tarih formatı hatalı. Örn: 2026-02-28");
                return false;
            }

            if (!TimeOnly.TryParseExact(startStr, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
            {
                ModelState.AddModelError(nameof(CreateClassSessionVm.StartTime), "Başlangıç saati formatı hatalı. Örn: 18:00");
                return false;
            }

            if (!TimeOnly.TryParseExact(endStr, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
            {
                ModelState.AddModelError(nameof(CreateClassSessionVm.EndTime), "Bitiş saati formatı hatalı. Örn: 19:00");
                return false;
            }

            if (end <= start)
            {
                ModelState.AddModelError(nameof(CreateClassSessionVm.EndTime), "Bitiş saati başlangıçtan büyük olmalı.");
                return false;
            }

            if (date < minDate || date > maxDate)
            {
                ModelState.AddModelError(nameof(CreateClassSessionVm.Date), $"Tarih {minDate:yyyy-MM-dd} ile {maxDate:yyyy-MM-dd} arasında olmalı.");
                return false;
            }

            return true;
        }

        private bool TryParseBulk(
            BulkCreateClassSessionsVm vm,
            out DateOnly fromDate,
            out DateOnly toDate,
            out TimeOnly start,
            out TimeOnly end,
            out HashSet<DayOfWeek> daySet)
        {
            daySet = new HashSet<DayOfWeek>();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var minDate = today.AddYears(-1);
            var maxDate = today.AddDays(364);

            if (!DateOnly.TryParseExact(vm.FromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate))
            {
                ModelState.AddModelError(nameof(vm.FromDate), "Başlangıç tarihi formatı hatalı. Örn: 2026-02-01");
                toDate = default; start = default; end = default;
                return false;
            }

            if (!DateOnly.TryParseExact(vm.ToDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
            {
                ModelState.AddModelError(nameof(vm.ToDate), "Bitiş tarihi formatı hatalı. Örn: 2026-02-28");
                start = default; end = default;
                return false;
            }

            if (toDate < fromDate)
            {
                ModelState.AddModelError(nameof(vm.ToDate), "Bitiş tarihi başlangıçtan küçük olamaz.");
                start = default; end = default;
                return false;
            }

            if (!TimeOnly.TryParseExact(vm.StartTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
            {
                ModelState.AddModelError(nameof(vm.StartTime), "Başlangıç saati formatı hatalı. Örn: 18:00");
                end = default;
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

            if (fromDate < minDate)
            {
                ModelState.AddModelError(nameof(vm.FromDate), $"Başlangıç tarihi {minDate:yyyy-MM-dd} tarihinden önce olamaz.");
                start = default; end = default;
                return false;
            }

            if (toDate > maxDate)
            {
                ModelState.AddModelError(nameof(vm.ToDate), $"Bitiş tarihi {maxDate:yyyy-MM-dd} tarihinden sonra olamaz.");
                start = default; end = default;
                return false;
            }

            if (toDate > fromDate.AddDays(364))
            {
                ModelState.AddModelError(nameof(vm.ToDate), "Tarih aralığı en fazla 52 hafta (364 gün) olabilir.");
                start = default; end = default;
                return false;
            }

            var count = 0;
            for (var d = fromDate; d <= toDate; d = d.AddDays(1))
            {
                if (daySet.Contains(d.DayOfWeek))
                    count++;
            }

            if (count > 365)
            {
                ModelState.AddModelError(string.Empty, "Toplu oluşturma en fazla 365 seans üretebilir. Tarih aralığını daralt veya gün seçimini azalt.");
                return false;
            }

            return true;
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
            var query = new Dictionary<string, string?>
            {
                ["ClassId"] = classId.ToString(),
                ["From"] = from.ToString("yyyy-MM-dd"),
                ["To"] = to.ToString("yyyy-MM-dd"),
                ["PageNumber"] = "1",
                ["PageSize"] = "200"
            };

            var url = QueryHelpers.AddQueryString("classsessions", query);

            var response = await _api.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
                return new List<ClassSessionListItemDto>();

            var result = await response.Content.ReadFromJsonAsync<PaginationResult<ClassSessionListItemDto>>(cancellationToken: ct);
            return result?.Items?.ToList() ?? new List<ClassSessionListItemDto>();
        }
    }
}
