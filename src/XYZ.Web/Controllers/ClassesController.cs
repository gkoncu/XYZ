using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using XYZ.Application.Features.Classes.Commands.AssignCoachToClass;
using XYZ.Application.Features.Classes.Commands.AssignStudentToClass;
using XYZ.Application.Features.Classes.Commands.CreateClass;
using XYZ.Application.Features.Classes.Commands.UnassignCoachToClass;
using XYZ.Application.Features.Classes.Commands.UnassignStudentFromClass;
using XYZ.Application.Features.Classes.Commands.UpdateClass;
using XYZ.Domain.Constants;
using XYZ.Web.Models.Classes;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = RoleNames.AdminCoachOrSuperAdmin)]
    public class ClassesController : Controller
    {
        private readonly IApiClient _api;

        public ClassesController(IApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? branchId,
            bool? isActive,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            var result = await _api.GetClassesAsync(searchTerm, branchId, isActive, pageNumber, pageSize, ct);

            var branches = await _api.GetBranchesAsync(1, 20, ct);

            ViewBag.BranchesSelectList = branches.Items
                .Select(b => new SelectListItem(b.Name, b.Id.ToString(), branchId == b.Id))
                .ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.BranchId = branchId;
            ViewBag.IsActive = isActive;

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct = default)
        {
            var dto = await _api.GetClassAsync(id, ct);
            if (dto is null) return NotFound();

            var students = await _api.GetStudentsAsync(null, 1, 20, ct);
            var coaches = await _api.GetCoachesAsync(null, 1, 20, ct);

            var assignedStudentIds = dto.Students.Select(s => s.Id).ToHashSet();
            var assignedCoachIds = dto.Coaches.Select(c => c.Id).ToHashSet();

            var vm = new ClassDetailsViewModel
            {
                Class = dto,
                AvailableStudents = students.Items
                    .Where(s => s.ClassId == null)
                    .Where(s => !assignedStudentIds.Contains(s.Id))
                    .Select(s => new SelectListItem($"{s.FullName}", s.Id.ToString()))
                    .ToList(),

                AvailableCoaches = coaches.Items
                    .Where(c => !assignedCoachIds.Contains(c.Id))
                    .Where(c => c.BranchId == dto.BranchId)
                    .Select(c => new SelectListItem($"{c.FullName}", c.Id.ToString()))
                    .ToList()
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Create(CancellationToken ct = default)
        {
            await FillBranchesSelectList(ct);
            return View(new ClassCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Create(ClassCreateViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                await FillBranchesSelectList(ct, model.BranchId);
                return View(model);
            }

            var id = await _api.CreateClassAsync(new CreateClassCommand
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                AgeGroupMin = model.AgeGroupMin,
                AgeGroupMax = model.AgeGroupMax,
                MaxCapacity = model.MaxCapacity,
                BranchId = model.BranchId
            }, ct);

            TempData["SuccessMessage"] = "Sınıf oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
        {
            var dto = await _api.GetClassAsync(id, ct);
            if (dto is null) return NotFound();

            await FillBranchesSelectList(ct, dto.BranchId);

            var vm = new ClassEditViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                AgeGroupMin = dto.AgeGroupMin,
                AgeGroupMax = dto.AgeGroupMax,
                MaxCapacity = dto.MaxCapacity,
                BranchId = dto.BranchId,
                IsActive = dto.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Edit(int id, ClassEditViewModel model, CancellationToken ct = default)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await FillBranchesSelectList(ct, model.BranchId);
                return View(model);
            }

            if (model.AgeGroupMin.HasValue && model.AgeGroupMax.HasValue && model.AgeGroupMin > model.AgeGroupMax)
            {
                ModelState.AddModelError(string.Empty, "Yaş Min, Yaş Max değerinden büyük olamaz.");
                await FillBranchesSelectList(ct, model.BranchId);
                return View(model);
            }

            await _api.UpdateClassAsync(id, new UpdateClassCommand
            {
                ClassId = id,
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                AgeGroupMin = model.AgeGroupMin,
                AgeGroupMax = model.AgeGroupMax,
                MaxCapacity = model.MaxCapacity,
                BranchId = model.BranchId
            }, ct);

            TempData["SuccessMessage"] = "Sınıf güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            await _api.DeleteClassAsync(id, ct);
            TempData["SuccessMessage"] = "Sınıf silindi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudent(int id, int studentId, CancellationToken ct = default)
        {
            try
            {
                await _api.AssignStudentToClassAsync(id, new AssignStudentToClassCommand
                {
                    ClassId = id,
                    StudentId = studentId
                }, ct);

                TempData["SuccessMessage"] = "Öğrenci sınıfa atandı.";
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.StatusCode == HttpStatusCode.Forbidden
                    ? "Bu işlem için yetkiniz yok."
                    : "Öğrenci sınıfa atanamadı.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignStudent(int id, int studentId, CancellationToken ct = default)
        {
            try
            {
                await _api.UnassignStudentFromClassAsync(id, new UnassignStudentFromClassCommand
                {
                    ClassId = id,
                    StudentId = studentId
                }, ct);

                TempData["SuccessMessage"] = "Öğrenci sınıftan çıkarıldı.";
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.StatusCode == HttpStatusCode.Forbidden
                    ? "Bu işlem için yetkiniz yok."
                    : "Öğrenci sınıftan çıkarılamadı.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> AssignCoach(int id, int coachId, CancellationToken ct = default)
        {
            try
            {
                await _api.AssignCoachToClassAsync(id, new AssignCoachToClassCommand
                {
                    ClassId = id,
                    CoachId = coachId
                }, ct);

                TempData["SuccessMessage"] = "Koç sınıfa atandı.";
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.StatusCode == HttpStatusCode.Forbidden
                    ? "Bu işlem için yetkiniz yok."
                    : "Koç atanamadı. Seçtiğiniz koç bu branşa ait değil.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> UnassignCoach(int id, int coachId, CancellationToken ct = default)
        {
            try
            {
                await _api.UnassignCoachFromClassAsync(id, new UnassignCoachFromClassCommand
                {
                    ClassId = id,
                    CoachId = coachId
                }, ct);

                TempData["SuccessMessage"] = "Koç sınıftan çıkarıldı.";
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.StatusCode == HttpStatusCode.Forbidden
                    ? "Bu işlem için yetkiniz yok."
                    : "Koç sınıftan çıkarılamadı.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task FillBranchesSelectList(CancellationToken ct, int? selectedBranchId = null)
        {
            var branchesResult = await _api.GetBranchesAsync(pageNumber: 1, pageSize: 20, ct);

            var items = branchesResult.Items
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                    Selected = selectedBranchId.HasValue && x.Id == selectedBranchId.Value
                })
                .ToList();

            ViewBag.BranchesSelectList = items;
        }
    }
}
