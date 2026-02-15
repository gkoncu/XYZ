using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;
using XYZ.Web.Models.ProgressRecords;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class ProgressRecordsController : Controller
    {
        private readonly IApiClient _apiClient;

        public ProgressRecordsController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminCoachOrSuperAdmin)]
        public async Task<IActionResult> Index(
            string? searchTerm,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var students = await _apiClient.GetStudentsAsync(
                searchTerm: searchTerm,
                pageNumber: pageNumber,
                pageSize: pageSize,
                cancellationToken: cancellationToken);

            ViewBag.SearchTerm = searchTerm;
            ViewData["Title"] = "Gelişim Kayıtları";

            return View(students);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminCoachStudentOrSuperAdmin)]
        public async Task<IActionResult> Student(
            int studentId,
            int? branchId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default)
        {
            if (studentId <= 0)
            {
                TempData["ErrorMessage"] = $"Geçersiz öğrenci id: {studentId}. URL query param adı 'studentId' olmalı.";
                return RedirectToAction("Index", "Students");
            }

            var student = await _apiClient.GetStudentAsync(studentId, cancellationToken);
            if (student is null)
                return NotFound();

            var branchOptions = await GetAllBranchesAsOptions(cancellationToken);

            if (!branchId.HasValue && student.BranchId.HasValue)
                branchId = student.BranchId.Value;

            IList<ProgressRecordListItemDto> items;
            try
            {
                items = await _apiClient.GetStudentProgressRecordsAsync(studentId, from, to, branchId, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                items = new List<ProgressRecordListItemDto>();
            }

            var canWrite = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Coach) || User.IsInRole(RoleNames.SuperAdmin);

            var vm = new StudentProgressRecordsViewModel
            {
                StudentId = studentId,
                StudentFullName = student.FullName,
                BranchId = branchId,
                BranchName = branchOptions.FirstOrDefault(x => x.Id == branchId).Name,
                From = from,
                To = to,
                BranchOptions = branchOptions,
                Items = items
                    .OrderByDescending(x => x.RecordDate)
                    .ThenByDescending(x => x.Sequence)
                    .ToList(),
                CanWrite = canWrite
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminCoachStudentOrSuperAdmin)]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var dto = await _apiClient.GetProgressRecordAsync(id, cancellationToken);
            if (dto is null)
                return NotFound();

            ViewBag.CanWrite = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Coach) || User.IsInRole(RoleNames.SuperAdmin);
            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminCoachOrSuperAdmin)]
        public async Task<IActionResult> Create(int studentId, int? branchId, CancellationToken cancellationToken = default)
        {
            var student = await _apiClient.GetStudentAsync(studentId, cancellationToken);
            if (student is null)
                return NotFound();

            var resolvedBranchId = student.BranchId ?? 0;

            var vm = new ProgressRecordCreateViewModel
            {
                StudentId = studentId,
                StudentFullName = student.FullName,
                BranchId = resolvedBranchId,
                BranchName = student.BranchName,
                RecordDate = DateOnly.FromDateTime(DateTime.Today)
            };

            if (vm.BranchId > 0)
                await FillMetricsForBranch(vm, vm.BranchId, cancellationToken);

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.AdminCoachOrSuperAdmin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int studentId,
            ProgressRecordCreateViewModel model,
            CancellationToken cancellationToken = default)
        {
            model.StudentId = studentId;

            var student = await _apiClient.GetStudentAsync(studentId, cancellationToken);
            if (student is null)
                return NotFound();

            model.BranchId = student.BranchId ?? 0;
            model.BranchName = student.BranchName;

            if (model.BranchId <= 0)
            {
                ModelState.AddModelError(nameof(model.BranchId), "Öğrenci bir sınıfa/branşa bağlı değil. Önce öğrenciyi bir sınıfa atayın.");
                return View(model);
            }

            await FillMetricsForBranch(model, model.BranchId, cancellationToken, keepPostedValues: true);

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var cmd = new CreateProgressRecordCommand
                {
                    StudentId = model.StudentId,
                    BranchId = model.BranchId,
                    RecordDate = model.RecordDate,
                    CoachNotes = model.CoachNotes,
                    Goals = model.Goals,
                    Values = model.Metrics
                        .Select(m => new Application.Features.ProgressRecords.Commands.CreateProgressRecord.MetricValueInput
                        {
                            ProgressMetricDefinitionId = m.ProgressMetricDefinitionId,
                            DecimalValue = m.DecimalValue,
                            IntValue = m.IntValue,
                            TextValue = m.TextValue
                        })
                        .ToList()
                };

                await _apiClient.CreateProgressRecordAsync(cmd, cancellationToken);

                TempData["SuccessMessage"] = "Gelişim kaydı eklendi.";

                return RedirectToAction("Student", new { studentId = studentId, branchId = model.BranchId });
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] =
                    ex.StatusCode == HttpStatusCode.BadRequest
                        ? "Kayıt oluşturulamadı. Lütfen alanları kontrol edin."
                        : "Kayıt oluşturulamadı. Lütfen tekrar deneyin.";

                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminCoachOrSuperAdmin)]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            var dto = await _apiClient.GetProgressRecordAsync(id, cancellationToken);
            if (dto is null)
                return NotFound();

            var student = await _apiClient.GetStudentAsync(dto.StudentId, cancellationToken);
            if (student is null)
                return NotFound();

            var defs = await _apiClient.GetProgressMetricDefinitionsAsync(dto.BranchId, includeInactive: true, cancellationToken);

            var vm = new ProgressRecordEditViewModel
            {
                Id = dto.Id,
                StudentId = dto.StudentId,
                StudentFullName = student.FullName,
                BranchId = dto.BranchId,
                BranchName = dto.BranchName,
                RecordDate = dto.RecordDate,
                Sequence = dto.Sequence,
                CreatedByDisplayName = dto.CreatedByDisplayName,
                CoachNotes = dto.CoachNotes,
                Goals = dto.Goals,
                Metrics = defs
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Name)
                    .Select(d =>
                    {
                        var current = dto.Values.FirstOrDefault(v => v.ProgressMetricDefinitionId == d.Id);

                        return new ProgressRecordMetricInputViewModel
                        {
                            ProgressMetricDefinitionId = d.Id,
                            MetricName = d.Name,
                            DataType = d.DataType,
                            Unit = d.Unit,
                            IsRequired = false,
                            DecimalValue = current?.DecimalValue,
                            IntValue = current?.IntValue,
                            TextValue = current?.TextValue
                        };
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.AdminCoachOrSuperAdmin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            ProgressRecordEditViewModel model,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var cmd = new UpdateProgressRecordCommand
                {
                    Id = model.Id,
                    CoachNotes = model.CoachNotes,
                    Goals = model.Goals,
                    Values = model.Metrics
                        .Select(m => new XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord.MetricValueInput
                        {
                            ProgressMetricDefinitionId = m.ProgressMetricDefinitionId,
                            DecimalValue = m.DecimalValue,
                            IntValue = m.IntValue,
                            TextValue = m.TextValue
                        })
                        .ToList()
                };

                await _apiClient.UpdateProgressRecordAsync(model.Id, cmd, cancellationToken);

                TempData["SuccessMessage"] = "Gelişim kaydı güncellendi.";
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Gelişim kaydı güncellenemedi. Lütfen tekrar deneyin.";
                return View(model);
            }
        }

        private async Task<List<(int Id, string Name)>> GetAllBranchesAsOptions(CancellationToken ct)
        {
            var result = await _apiClient.GetBranchesAsync(pageNumber: 1, pageSize: 500, ct);
            return result.Items
                .OrderBy(x => x.Name)
                .Select(x => (x.Id, x.Name))
                .ToList();
        }

        private async Task FillMetricsForBranch(
            ProgressRecordCreateViewModel model,
            int branchId,
            CancellationToken ct,
            bool keepPostedValues = false)
        {
            if (branchId <= 0)
            {
                model.Metrics = new List<ProgressRecordMetricInputViewModel>();
                return;
            }

            var defs = await _apiClient.GetProgressMetricDefinitionsAsync(branchId, includeInactive: false, ct);

            var posted = keepPostedValues
                ? model.Metrics.ToDictionary(x => x.ProgressMetricDefinitionId, x => x)
                : new Dictionary<int, ProgressRecordMetricInputViewModel>();

            model.Metrics = defs
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(d =>
                {
                    posted.TryGetValue(d.Id, out var p);

                    return new ProgressRecordMetricInputViewModel
                    {
                        ProgressMetricDefinitionId = d.Id,
                        MetricName = d.Name,
                        DataType = d.DataType,
                        Unit = d.Unit,
                        IsRequired = false,
                        DecimalValue = p?.DecimalValue,
                        IntValue = p?.IntValue,
                        TextValue = p?.TextValue
                    };
                })
                .ToList();
        }
    }
}
