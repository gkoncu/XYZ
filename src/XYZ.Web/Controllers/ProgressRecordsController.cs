using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord;
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
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> Student(
            int studentId,
            DateTime? from,
            DateTime? to,
            CancellationToken cancellationToken = default)
        {
            var student = await _apiClient.GetStudentAsync(studentId, cancellationToken);
            if (student is null)
            {
                return NotFound();
            }

            var items = await _apiClient.GetStudentProgressRecordsAsync(
                studentId,
                from,
                to,
                cancellationToken);

            var vm = new StudentProgressRecordsViewModel
            {
                StudentId = studentId,
                StudentFullName = student.FullName,
                From = from,
                To = to,
                Items = items
                    .OrderByDescending(x => x.RecordDate)
                    .ToList(),
                CanWrite = User.IsInRole("Admin") || User.IsInRole("Coach") || User.IsInRole("SuperAdmin")
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var dto = await _apiClient.GetProgressRecordAsync(id, cancellationToken);
            if (dto is null)
            {
                return NotFound();
            }

            ViewBag.CanWrite = User.IsInRole("Admin") || User.IsInRole("Coach") || User.IsInRole("SuperAdmin");
            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<IActionResult> Create(int studentId, CancellationToken cancellationToken = default)
        {
            var student = await _apiClient.GetStudentAsync(studentId, cancellationToken);
            if (student is null)
            {
                return NotFound();
            }

            var vm = new ProgressRecordCreateViewModel
            {
                StudentId = studentId,
                StudentFullName = student.FullName,
                RecordDate = DateTime.Today
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ProgressRecordCreateViewModel model,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _apiClient.CreateProgressRecordAsync(new CreateProgressRecordCommand
            {
                StudentId = model.StudentId,
                RecordDate = model.RecordDate,

                Height = model.Height,
                Weight = model.Weight,
                BodyFatPercentage = model.BodyFatPercentage,
                VerticalJump = model.VerticalJump,
                SprintTime = model.SprintTime,
                Endurance = model.Endurance,
                Flexibility = model.Flexibility,

                TechnicalScore = model.TechnicalScore,
                TacticalScore = model.TacticalScore,
                PhysicalScore = model.PhysicalScore,
                MentalScore = model.MentalScore,

                CoachNotes = string.IsNullOrWhiteSpace(model.CoachNotes) ? null : model.CoachNotes.Trim(),
                Goals = string.IsNullOrWhiteSpace(model.Goals) ? null : model.Goals.Trim()
            }, cancellationToken);

            TempData["SuccessMessage"] = "Gelişim kaydı eklendi.";
            return RedirectToAction(nameof(Student), new { studentId = model.StudentId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            var dto = await _apiClient.GetProgressRecordAsync(id, cancellationToken);
            if (dto is null)
            {
                return NotFound();
            }

            var vm = new ProgressRecordEditViewModel
            {
                Id = dto.Id,
                StudentId = dto.StudentId,
                StudentFullName = dto.StudentFullName,
                RecordDate = dto.RecordDate,

                Height = dto.Height,
                Weight = dto.Weight,
                BodyFatPercentage = dto.BodyFatPercentage,
                VerticalJump = dto.VerticalJump,
                SprintTime = dto.SprintTime,
                Endurance = dto.Endurance,
                Flexibility = dto.Flexibility,

                TechnicalScore = dto.TechnicalScore,
                TacticalScore = dto.TacticalScore,
                PhysicalScore = dto.PhysicalScore,
                MentalScore = dto.MentalScore,

                CoachNotes = dto.CoachNotes,
                Goals = dto.Goals
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProgressRecordEditViewModel model,
            CancellationToken cancellationToken = default)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _apiClient.UpdateProgressRecordAsync(id, new UpdateProgressRecordCommand
            {
                Id = id,
                RecordDate = model.RecordDate,

                Height = model.Height,
                Weight = model.Weight,
                BodyFatPercentage = model.BodyFatPercentage,
                VerticalJump = model.VerticalJump,
                SprintTime = model.SprintTime,
                Endurance = model.Endurance,
                Flexibility = model.Flexibility,

                TechnicalScore = model.TechnicalScore,
                TacticalScore = model.TacticalScore,
                PhysicalScore = model.PhysicalScore,
                MentalScore = model.MentalScore,

                CoachNotes = string.IsNullOrWhiteSpace(model.CoachNotes) ? null : model.CoachNotes.Trim(),
                Goals = string.IsNullOrWhiteSpace(model.Goals) ? null : model.Goals.Trim(),

                IsActive = null
            }, cancellationToken);

            TempData["SuccessMessage"] = "Gelişim kaydı güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int studentId, CancellationToken cancellationToken = default)
        {
            await _apiClient.DeleteProgressRecordAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Gelişim kaydı silindi.";

            return RedirectToAction(nameof(Student), new { studentId });
        }
    }
}
