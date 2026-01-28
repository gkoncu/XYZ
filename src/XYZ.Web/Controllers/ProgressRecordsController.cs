using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords;
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
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> Student(
            int studentId,
            DateTime? from,
            DateTime? to,
            CancellationToken cancellationToken = default)
        {
            if (studentId <= 0)
            {
                TempData["ErrorMessage"] = $"Geçersiz öğrenci id: {studentId}. URL query param adı 'studentId' olmalı.";
                return RedirectToAction("Index", "Students");
            }

            var student = await _apiClient.GetStudentAsync(studentId, cancellationToken);
            if (student is null)
            {
                return NotFound();
            }

            IList<ProgressRecordListItemDto> items;

            try
            {
                items = await _apiClient.GetStudentProgressRecordsAsync(studentId, from, to, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                items = new List<ProgressRecordListItemDto>();
            }

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
                CoachNotes = model.CoachNotes
            }, cancellationToken);

            TempData["SuccessMessage"] = "Gelişim kaydı eklendi.";
            return RedirectToAction("Student", new { studentId = model.StudentId });
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
                CoachNotes = dto.CoachNotes
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            ProgressRecordEditViewModel model,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var vm = new UpdateProgressRecordCommand
            {
                Id = model.Id,
                RecordDate = model.RecordDate,
                Height = model.Height,
                Weight = model.Weight,
                CoachNotes = model.CoachNotes
            };

            await _apiClient.UpdateProgressRecordAsync(vm.Id, vm, cancellationToken);

            TempData["SuccessMessage"] = "Gelişim kaydı güncellendi.";
            return RedirectToAction("Details", new { id = model.Id });
        }
    }
}
