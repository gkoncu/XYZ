using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Students.Commands.CreateStudent;
using XYZ.Application.Features.Students.Commands.UpdateStudent;
using XYZ.Application.Features.Students.Queries.GetAllStudents;
using XYZ.Application.Features.Students.Queries.GetStudentById;
using XYZ.Domain.Entities;
using XYZ.Web.Models.Students;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly IApiClient _apiClient;

        public StudentsController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var students = await _apiClient.GetStudentsAsync(
                searchTerm,
                pageNumber,
                pageSize,
                cancellationToken);

            ViewBag.SearchTerm = searchTerm;

            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var student = await _apiClient.GetStudentAsync(id, cancellationToken);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public IActionResult Create()
        {
            var vm = new StudentCreateViewModel
            {
                BirthDate = DateTime.Today.AddYears(-8),
                Gender = "PreferNotToSay",
                BloodType = "Unknown"
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            StudentCreateViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!model.BirthDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.BirthDate), "Doğum tarihi zorunludur.");
                return View(model);
            }

            var request = new CreateStudentRequestDTO
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate.Value,
                Gender = model.Gender,
                BloodType = model.BloodType,
                ClassId = model.ClassId,
                IdentityNumber = model.IdentityNumber,
                Address = model.Address,
                Parent1FirstName = model.Parent1FirstName,
                Parent1LastName = model.Parent1LastName,
                Parent1Email = model.Parent1Email,
                Parent1PhoneNumber = model.Parent1PhoneNumber,
                Parent2FirstName = model.Parent2FirstName,
                Parent2LastName = model.Parent2LastName,
                Parent2Email = model.Parent2Email,
                Parent2PhoneNumber = model.Parent2PhoneNumber,
                Notes = model.Notes,
                MedicalInformation = model.MedicalInformation
            };

            var response = await _apiClient.PostAsJsonAsync("students", request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                ModelState.AddModelError(string.Empty, $"API Hatası: {errorBody}");
                return View(model);
            }

            var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            if (id <= 0)
            {
                TempData["SuccessMessage"] = "Öğrenci oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Öğrenci oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _apiClient.GetStudentAsync(id, cancellationToken);
            if (dto == null)
            {
                return NotFound();
            }

            var fullName = dto.FullName?.Trim() ?? string.Empty;
            string firstName = fullName;
            string lastName = string.Empty;

            var lastSpace = fullName.LastIndexOf(' ');
            if (lastSpace > 0)
            {
                firstName = fullName.Substring(0, lastSpace);
                lastName = fullName.Substring(lastSpace + 1);
            }

            var vm = new StudentEditViewModel
            {
                Id = dto.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                BirthDate = dto.BirthDate,
                Gender = dto.Gender,
                BloodType = dto.BloodType,
                ClassId = dto.ClassId,
                IdentityNumber = dto.IdentityNumber,
                Address = dto.Address,
                Parent1FirstName = dto.Parent1FirstName,
                Parent1LastName = dto.Parent1LastName,
                Parent1Email = dto.Parent1Email,
                Parent1PhoneNumber = dto.Parent1PhoneNumber,
                Parent2FirstName = dto.Parent2FirstName,
                Parent2LastName = dto.Parent2LastName,
                Parent2Email = dto.Parent2Email,
                Parent2PhoneNumber = dto.Parent2PhoneNumber,
                Notes = dto.Notes,
                MedicalInformation = dto.MedicalInformation,
                IsActive = dto.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            StudentEditViewModel model,
            CancellationToken cancellationToken)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!model.BirthDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.BirthDate), "Doğum tarihi zorunludur.");
                return View(model);
            }

            var command = new UpdateStudentCommand
            {
                StudentId = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate.Value,
                Gender = model.Gender,
                BloodType = model.BloodType,
                ClassId = model.ClassId,
                IdentityNumber = model.IdentityNumber,
                Address = model.Address,
                Parent1FirstName = model.Parent1FirstName,
                Parent1LastName = model.Parent1LastName,
                Parent1Email = model.Parent1Email,
                Parent1PhoneNumber = model.Parent1PhoneNumber,
                Parent2FirstName = model.Parent2FirstName,
                Parent2LastName = model.Parent2LastName,
                Parent2Email = model.Parent2Email,
                Parent2PhoneNumber = model.Parent2PhoneNumber,
                Notes = model.Notes,
                MedicalInformation = model.MedicalInformation
            };

            var response = await _apiClient.PutAsJsonAsync($"students/{id}", command, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                ModelState.AddModelError(string.Empty, $"API Hatası: {errorBody}");
                return View(model);
            }

            TempData["SuccessMessage"] = "Öğrenci bilgileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var response = await _apiClient.DeleteAsync($"students/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Öğrenci silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Öğrenci silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
