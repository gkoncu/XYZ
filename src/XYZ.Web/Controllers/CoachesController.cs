using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Coaches.Commands.CreateCoach;
using XYZ.Application.Features.Coaches.Commands.UpdateCoach;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;
using XYZ.Application.Features.Coaches.Queries.GetCoachById;
using XYZ.Web.Models.Coaches;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class CoachesController : Controller
    {
        private readonly IApiClient _apiClient;

        public CoachesController(IApiClient apiClient)
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
            var result = await _apiClient.GetCoachesAsync(
                searchTerm,
                pageNumber,
                pageSize,
                cancellationToken);

            ViewBag.SearchTerm = searchTerm;

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var coach = await _apiClient.GetCoachAsync(id, cancellationToken);
            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult Create()
        {
            var vm = new CoachCreateViewModel();
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CoachCreateViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var command = new CreateCoachCommand
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Branch = model.Branch,
                Bio = model.Bio,
                Notes = model.Notes
            };

            var response = await _apiClient.PostAsJsonAsync("coaches", command, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Koç kaydedilirken bir hata oluştu.");
                return View(model);
            }

            var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            if (id <= 0)
            {
                TempData["SuccessMessage"] = "Koç oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Koç oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _apiClient.GetCoachAsync(id, cancellationToken);
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

            var vm = new CoachEditViewModel
            {
                Id = dto.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Branch = dto.Branch,
                Bio = dto.Bio,
                Notes = dto.Notes,
                IsActive = dto.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CoachEditViewModel model,
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

            var command = new UpdateCoachCommand
            {
                CoachId = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Branch = model.Branch,
                Bio = model.Bio,
                Notes = model.Notes
            };

            var response = await _apiClient.PutAsJsonAsync($"coaches/{id}", command, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Koç güncellenirken bir hata oluştu.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Koç bilgileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var response = await _apiClient.DeleteAsync($"coaches/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Koç silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Koç silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
