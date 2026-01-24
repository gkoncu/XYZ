using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Coaches.Commands.UpdateCoach;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;
using XYZ.Application.Features.Coaches.Queries.GetCoachById;
using XYZ.Domain.Entities;
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
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<IActionResult> Index(string searchTerm, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var coaches = await _apiClient.GetCoachesAsync(searchTerm, pageNumber, pageSize, cancellationToken);

            var onlyIncompleteDocs = Request.Query.ContainsKey("onlyIncompleteDocs")
                         && string.Equals(Request.Query["onlyIncompleteDocs"], "true", StringComparison.OrdinalIgnoreCase);

            ViewBag.OnlyIncompleteDocs = onlyIncompleteDocs;

            var statusPairs = await Task.WhenAll(
                coaches.Items.Select(async c =>
                {
                    try
                    {
                        var st = await _apiClient.GetCoachDocumentStatusAsync(c.Id, cancellationToken);
                        return (CoachId: c.Id, IsComplete: st.IsComplete, MissingCount: st.MissingCount);
                    }
                    catch
                    {
                        return (CoachId: c.Id, IsComplete: true, MissingCount: 0);
                    }
                })
            );

            var docStatusMap = statusPairs.ToDictionary(x => x.CoachId, x => (x.IsComplete, x.MissingCount));

            if (onlyIncompleteDocs)
            {
                coaches.Items = coaches.Items
                    .Where(c => docStatusMap.TryGetValue(c.Id, out var v) && v.MissingCount > 0)
                    .ToList();
            }

            ViewBag.CoachDocStatusMap = docStatusMap;

            return View(coaches);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var branches = await _apiClient.GetBranchesAsync(1, 50, cancellationToken);

            ViewBag.Branches = branches.Items;

            var vm = new CoachCreateViewModel
            {
                BirthDate = DateTime.Today.AddYears(-18),
                Gender = "PreferNotToSay",
                BloodType = "Unknown"
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CoachCreateViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var branches = await _apiClient.GetBranchesAsync(1, 50, cancellationToken);
                ViewBag.Branches = branches.Items;
                return View(model);
            }

            if (!model.BirthDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.BirthDate), "Doğum tarihi zorunludur.");
                var branches = await _apiClient.GetBranchesAsync(1, 50, cancellationToken);
                ViewBag.Branches = branches.Items;
                return View(model);
            }

            var request = new CreateCoachRequestDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate.Value,
                Gender = model.Gender,
                BloodType = model.BloodType,
                BranchId = model.BranchId,
                IdentityNumber = model.IdentityNumber,
                LicenseNumber = model.LicenseNumber
            };

            var response = await _apiClient.PostAsJsonAsync("coaches", request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                ModelState.AddModelError(string.Empty, $"API Hatası: {errorBody}");

                var branches = await _apiClient.GetBranchesAsync(1, 50, cancellationToken);
                ViewBag.Branches = branches.Items;

                return View(model);
            }

            var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            TempData["SuccessMessage"] = "Koç oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _apiClient.GetCoachAsync(id, cancellationToken);
            if (dto == null)
                return NotFound();

            var branches = await _apiClient.GetBranchesAsync(1, 50, cancellationToken);
            ViewBag.Branches = branches.Items;

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
                Gender = dto.Gender,
                BloodType = dto.BloodType,
                BirthDate = dto.BirthDate,
                IdentityNumber = dto.IdentityNumber,
                LicenseNumber = dto.LicenseNumber,
                BranchId = dto.BranchId
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CoachEditViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                var branches = await _apiClient.GetBranchesAsync(1, 50, cancellationToken);
                ViewBag.Branches = branches.Items;
                return View(model);
            }

            if (!model.BirthDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.BirthDate), "Doğum tarihi zorunludur.");
                var branches = await _apiClient.GetBranchesAsync(1, 50, cancellationToken);
                ViewBag.Branches = branches.Items;
                return View(model);
            }

            var command = new UpdateCoachCommand
            {
                CoachId = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate.Value,
                Gender = model.Gender,
                BloodType = model.BloodType,
                IdentityNumber = model.IdentityNumber,
                LicenseNumber = model.LicenseNumber,
                BranchId = model.BranchId
            };

            var response = await _apiClient.PutAsJsonAsync($"coaches/{id}", command, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                ModelState.AddModelError(string.Empty, $"API Hatası: {errorBody}");

                var branches = await _apiClient.GetBranchesAsync(1, 50, cancellationToken);
                ViewBag.Branches = branches.Items;

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
                return RedirectToAction("Login", "Account");

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                TempData["ErrorMessage"] = $"API Hatası: {errorBody}";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Koç silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
