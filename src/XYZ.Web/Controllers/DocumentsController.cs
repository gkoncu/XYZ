using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XYZ.Application.Features.Documents.Queries.DocumentStatus;
using XYZ.Web.Models.Documents;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly IApiClient _api;

        public DocumentsController(IApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<IActionResult> Students(
            string? searchTerm,
            bool onlyIncomplete = true,
            int take = 200,
            CancellationToken ct = default)
        {
            var items = await _api.GetStudentsDocumentStatusAsync(
                onlyIncomplete,
                searchTerm,
                Math.Clamp(take, 1, 1000),
                ct);

            items = items
                .OrderByDescending(x => x.MissingCount)
                .ThenBy(x => x.FullName)
                .ToList();

            var vm = new DocumentOwnerIndexViewModel<StudentDocumentStatusListItemDto>
            {
                Title = "Öğrenci Belgeleri",
                SearchTerm = searchTerm,
                OnlyIncomplete = onlyIncomplete,
                Items = items
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Coaches(
            string? searchTerm,
            bool onlyIncomplete = true,
            int take = 200,
            CancellationToken ct = default)
        {
            var items = await _api.GetCoachesDocumentStatusAsync(
                onlyIncomplete,
                searchTerm,
                Math.Clamp(take, 1, 1000),
                ct);

            items = items
                .OrderByDescending(x => x.MissingCount)
                .ThenBy(x => x.FullName)
                .ToList();

            var vm = new DocumentOwnerIndexViewModel<CoachDocumentStatusListItemDto>
            {
                Title = "Koç Belgeleri",
                SearchTerm = searchTerm,
                OnlyIncomplete = onlyIncomplete,
                Items = items
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> Student(
            int studentId,
            int? type,
            CancellationToken ct = default)
        {
            if (studentId <= 0) return BadRequest();

            var status = await _api.GetStudentDocumentStatusAsync(studentId, ct);
            var docs = await _api.GetStudentDocumentsAsync(studentId, type, ct);

            var vm = new UserDocumentsViewModel
            {
                Title = "Öğrenci Belgeleri",
                OwnerId = studentId,
                Status = status,
                Documents = docs.OrderByDescending(d => d.UploadDate).ToList(),
                TypeFilter = type
            };

            vm.DocumentDefinitionOptions = status.Missing
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new SelectListItem($"{x.Name} (Eksik)", x.DocumentDefinitionId.ToString()))
                .ToList();

            return View("UserDocuments", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> UploadStudent(
            int studentId,
            UserDocumentsViewModel model,
            CancellationToken ct = default)
        {
            if (studentId <= 0) return BadRequest();

            if (model.File is null || model.File.Length == 0)
            {
                TempData["ErrorMessage"] = "Dosya seçmelisiniz.";
                return RedirectToAction(nameof(Student), new { studentId });
            }

            if (model.DocumentDefinitionId <= 0)
            {
                TempData["ErrorMessage"] = "Belge türü seçmelisiniz.";
                return RedirectToAction(nameof(Student), new { studentId });
            }

            await using var stream = model.File.OpenReadStream();

            await _api.UploadStudentDocumentAsync(
                studentId,
                stream,
                model.File.FileName,
                model.Name,
                model.Description,
                model.DocumentDefinitionId,
                ct);

            TempData["SuccessMessage"] = "Belge yüklendi.";
            return RedirectToAction(nameof(Student), new { studentId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Coach")]
        public async Task<IActionResult> Coach(
            int coachId,
            int? type,
            CancellationToken ct = default)
        {
            if (coachId <= 0) return BadRequest();

            var status = await _api.GetCoachDocumentStatusAsync(coachId, ct);
            var docs = await _api.GetCoachDocumentsAsync(coachId, type, ct);

            var vm = new UserDocumentsViewModel
            {
                Title = "Koç Belgeleri",
                OwnerId = coachId,
                Status = status,
                Documents = docs.OrderByDescending(d => d.UploadDate).ToList(),
                TypeFilter = type
            };

            vm.DocumentDefinitionOptions = status.Missing
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new SelectListItem($"{x.Name} (Eksik)", x.DocumentDefinitionId.ToString()))
                .ToList();

            return View("UserDocuments", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin,Coach")]
        public async Task<IActionResult> UploadCoach(
            int coachId,
            UserDocumentsViewModel model,
            CancellationToken ct = default)
        {
            if (coachId <= 0) return BadRequest();

            if (model.File is null || model.File.Length == 0)
            {
                TempData["ErrorMessage"] = "Dosya seçmelisiniz.";
                return RedirectToAction(nameof(Coach), new { coachId });
            }

            if (model.DocumentDefinitionId <= 0)
            {
                TempData["ErrorMessage"] = "Belge türü seçmelisiniz.";
                return RedirectToAction(nameof(Coach), new { coachId });
            }

            await using var stream = model.File.OpenReadStream();

            await _api.UploadCoachDocumentAsync(
                coachId,
                stream,
                model.File.FileName,
                model.Name,
                model.Description,
                model.DocumentDefinitionId,
                ct);

            TempData["SuccessMessage"] = "Belge yüklendi.";
            return RedirectToAction(nameof(Coach), new { coachId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> Download(int id, CancellationToken ct = default)
        {
            var streamResult = await _api.DownloadDocumentAsync(id, ct);
            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> Delete(
            int id,
            string returnTo,
            int ownerId,
            CancellationToken ct = default)
        {
            if (id <= 0) return BadRequest();

            await _api.DeleteDocumentAsync(id, ct);
            TempData["SuccessMessage"] = "Belge silindi.";

            return returnTo?.ToLowerInvariant() switch
            {
                "coach" => RedirectToAction(nameof(Coach), new { coachId = ownerId }),
                _ => RedirectToAction(nameof(Student), new { studentId = ownerId })
            };
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> My(CancellationToken ct = default)
        {
            var me = await _api.GetMyProfileAsync(ct);

            if (me?.StudentProfileId is null || me.StudentProfileId.Value <= 0)
                return Forbid();

            return RedirectToAction(nameof(Student), new { studentId = me.StudentProfileId.Value });
        }

    }
}
